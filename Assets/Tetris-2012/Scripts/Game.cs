/*
===============================================================================
    Copyright (C) 2020 Ilya Lyakhovets
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
===============================================================================
*/

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using TMPro;

namespace IlyaLts.Tetris
{
    public class Game : MonoBehaviour
    {
        const int fieldWidth = 10;
        const int fieldHeight = 20;
        const float keyDelay = 0.18f;
        const float figureHelperAlpha = 0.33f;

        const float figureFallStartDelay = 1.0f;
        const float figureFallDecreaseCoeff = 0.4f;
        const float defaultKeyDelay = 0.18f;
        const bool defaultSoundEnabled = true;
        const bool defaultHelperEnabled = false;

        const int scoreFor1Line = 10;
        const int scoreFor2Lines = 25;
        const int scoreFor3Lines = 50;
        const int scoreFor4Lines = 100;
        const int startupScoreGoal = 100;
        const int scoreGoalMultiplier = 2;
        const int maxScore = 99999999;

        public TMP_Text textScoreCount;
        public TMP_Text textGoalCount;
        public TMP_Text textLevelCount;
        public TMP_Text textRecordCount;
        public AudioClip soundDrop;
        public GameObject PanelGameOver;
        public GameObject panelHelp;
        public GameObject block;
        public Sprite[] blocks;

        [SerializeField]
        bool soundEnabled = true;
        [SerializeField]
        bool figureHelperEnabled = true;

        bool gameOver;
        int score;
        int scoreGoal;
        int level;
        int record;

        Figure figure = new Figure();
        GameObject[,] field = new GameObject[fieldWidth, fieldHeight];
        GameObject[,] figureNext = new GameObject[Figure.width, Figure.height];
        GameObject[,] figureHelper = new GameObject[Figure.width, Figure.height];

        void CreateBlock(ref GameObject block, Vector3 pos, int color, String parentName, float alpha = 1.0f)
        {
            block = Instantiate(this.block, pos, Quaternion.identity);
            block.GetComponent<Image>().sprite = blocks[color];
            block.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, alpha);
            block.transform.SetParent(GameObject.Find(parentName).transform, false);
        }

        void DestroyBlock(ref GameObject block)
        {
            if (block)
            {
                Destroy(block.gameObject);
                block = null;
            }
        }

        void CreateFigure()
        {
            for (int i = 0; i < Figure.width; i++)
            {
                for (int j = 0; j < Figure.height; j++)
                {
                    if (Figure.figures[figure.num, figure.rot, j, i] == 1)
                    {
                        Rect rect = block.GetComponent<RectTransform>().rect;
                        CreateBlock(ref figure.blocks[i, j], new Vector3(rect.width * (i + figure.x), -rect.height * (j + figure.y), 0.0f), figure.num, "Field");
                    }
                }
            }
        }

        void DestroyFigure()
        {
            for (int i = 0; i < Figure.width; i++)
            {
                for (int j = 0; j < Figure.height; j++)
                {
                    if (figure.blocks[i, j])
                    {
                        DestroyBlock(ref figure.blocks[i, j]);
                    }
                }
            }
        }

        void UpdateNextFigure()
        {
            for (int i = 0; i < Figure.width; i++)
            {
                for (int j = 0; j < Figure.height; j++)
                {
                    DestroyBlock(ref figureNext[i, j]);

                    if (Figure.figures[figure.numNext, 0, j, i] == 1)
                    {
                        Rect rect = block.GetComponent<RectTransform>().rect;
                        CreateBlock(ref figureNext[i, j], new Vector3(rect.width * i, -rect.height * j, 0.0f), figure.numNext, "PanelNextFigure");
                    }
                }
            }
        }

        void UpdateFigureHelper()
        {
            int tempY = figure.y;
                            
            while (!IsFigureDropped()) figure.y++;

            for (int i = 0; i < Figure.width; i++)
            {
                for (int j = 0; j < Figure.height; j++)
                {
                    DestroyBlock(ref figureHelper[i, j]);

                    // Figure helper is shown only if the current figure is higher than its height
                    if (figureHelperEnabled && figure.blocks[i, j] && tempY + Figure.height < figure.y)
                    {
                        Rect rect = block.GetComponent<RectTransform>().rect;
                        CreateBlock(ref figureHelper[i, j], new Vector3(rect.width * (i + figure.x), -rect.height * (j + figure.y), 0.0f), figure.num, "Field", figureHelperAlpha);
                    }
                }
            }

			figure.y = tempY;
        }

        void NewGame()
        {
            for (int i = 0; i < fieldWidth; i++)
                for (int j = 0; j < fieldHeight; j++)
                    DestroyBlock(ref field[i, j]);

            StopAllCoroutines();
            DestroyFigure();
            figure.New((fieldWidth / 2) - (Figure.width / 2), 0);
            UpdateNextFigure();
            CreateFigure();
            UpdateFigureHelper();

            PanelGameOver.GetComponentInChildren<TMP_Text>(true).SetText("GAME OVER!");
            PanelGameOver.SetActive(false);
            gameOver = false;
            score = 0;
            scoreGoal = startupScoreGoal;
            level = 1;

            textScoreCount.text = "0";
            textGoalCount.text = Convert.ToString(scoreGoal);
            textLevelCount.text = Convert.ToString(level);

            StartCoroutine("FallRoutine");
            StartCoroutine("MoveLeftRoutine");
            StartCoroutine("MoveRightRoutine");
            StartCoroutine("MoveDownRoutine");
        }

        void AddFigureIntoField()
        {
            for (int i = 0; i < Figure.width; i++)
            {
                for (int j = 0; j < Figure.height; j++)
                {
                    if (figure.blocks[i, j])
                    {
                        field[figure.x + i, figure.y + j] = figure.blocks[i, j];
                    }
                }
            }
        }

        void RemoveFilledLines()
        {
            int numOfFilledLines = 0;

            for (int i = fieldHeight - 1; i > 0; i--)
            {
                bool filled = true;

                for (int j = 0; j < fieldWidth; j++)
                {
                    if (!field[j, i])
                    {
                        filled = false;
                    }
                }

                if (filled)
                {
                    for (int j = 0; j < fieldWidth; j++)
                        DestroyBlock(ref field[j, i]);

                    // Moves upper lines down
                    for (int len = i; len > 0; len--)
                    {
                        for (int j = 0; j < fieldWidth; j++)
                        {
                            if (field[j, len - 1])
                            {
                                field[j, len] = field[j, len - 1];
                                field[j, len - 1] = null;
                                field[j, len].GetComponent<Block>().Move(0, -1);
                            }
                        }
                    }

                    i++;
                    numOfFilledLines++;
                }
            }

            // Scoring
            if (numOfFilledLines > 0)
            {
                if (numOfFilledLines == 1)
                    score += scoreFor1Line * level;
                else if (numOfFilledLines == 2)
                    score += scoreFor2Lines * level;
                else if (numOfFilledLines == 3)
                    score += scoreFor3Lines * level;
                else
                    score += scoreFor4Lines * level;

                if (score >= maxScore)
                {
                    score = maxScore;
                    scoreGoal = maxScore;
                    gameOver = true;
                    PanelGameOver.GetComponentInChildren<TMP_Text>(true).SetText("MAX SCORE!");
                    PanelGameOver.SetActive(true);
                    StopAllCoroutines();
                    return;
                }
                else if (score >= scoreGoal)
                {
                    level += 1;
                    scoreGoal *= scoreGoalMultiplier;
                    if (scoreGoal > maxScore) scoreGoal = maxScore;
                }

                textScoreCount.text = Convert.ToString(score);
                textGoalCount.text = Convert.ToString(scoreGoal);
                textLevelCount.text = Convert.ToString(level);
            }
        }

        bool IsFigureDropped()
        {
            for (int i = 0; i < Figure.width; i++)
            {
                for (int j = 0; j < Figure.height; j++)
                {
                    if (figure.blocks[i, j])
                    {
                        if (j + 1 < Figure.height && figure.blocks[i, j + 1] && field[figure.x + i, figure.y + j + 1])
                            continue;

                        if (figure.y + j + 1 == fieldHeight || field[figure.x + i, figure.y + j + 1])
                            return true;
                    }
                }
            }

            return false;
        }

        bool IsThereFreeSpaceForNewFigure()
        {
            for (int i = 0; i < Figure.width; i++)
                for (int j = 0; j < Figure.height; j++)
                    if (figure.blocks[i, j] && field[figure.x + i, figure.y + j])
                        return false;

            return true;
        }

        public void MoveLeft()
        {
            for (int i = 0; i < Figure.width; i++)
            {
                for (int j = 0; j < Figure.height; j++)
                {
                    if (figure.x + i <= 0 && figure.blocks[i, j])
                        return;

                    if (figure.blocks[i, j] && field[figure.x + i - 1, figure.y + j])
                        return;
                }
            }

            figure.Move(-1, 0);
            UpdateFigureHelper();
        }

        public void MoveRight()
        {
            for (int i = Figure.width - 1; i >= 0; i--)
            {
                for (int j = 0; j < Figure.height; j++)
                {
                    if (figure.x + i >= fieldWidth - 1 && figure.blocks[i, j])
                        return;

                    if (figure.blocks[i, j] && field[figure.x + i + 1, figure.y + j])
                        return;
                }
            }

            figure.Move(1, 0);
            UpdateFigureHelper();
        }

        public void MoveDown()
        {
            if (!IsFigureDropped())
            {
                figure.Move(0, -1);
                UpdateFigureHelper();
            }
        }

        public void DropFigure()
        {
            while (!IsFigureDropped()) MoveDown();
        }

        public void RotateFigure()
        {
            bool flag;
            bool allowRotate = true;
            int xOld = figure.x;

            int rot = figure.rot + 1;
            if (rot == Figure.numOfRotations) rot = 0;

            // Moves the figure to the right from the wall if there is not enough free space for rotations
            do
            {
                flag = true;

                for (int i = 0; i < -figure.x; i++)
                {
                    for (int j = 0; j < Figure.height; j++)
                    {
                        if (Figure.figures[figure.num, rot, j, i] == 1)
                        {
                            flag = false;
                        }
                    }
                }

                if (!flag) MoveRight();
            } while (!flag);

            // Moves the figure to the left from the wall if there is not enough free space for rotations
            do
            {
                flag = true;

                for (int i = Figure.width - 1; i > fieldWidth - 1 - figure.x; i--)
                {
                    for (int j = 0; j < Figure.height; j++)
                    {
                        if (Figure.figures[figure.num, rot, j, i] == 1)
                        {
                            flag = false;
                        }
                    }
                }

                if (!flag) MoveLeft();
            } while (!flag);

            for (int i = 0; i < Figure.width; i++)
            {
                for (int j = 0; j < Figure.height; j++)
                {
                    // Doesn't allow rotations of the figure when it's too low
                    if (Figure.figures[figure.num, rot, j, i] == 1 && figure.y + j >= fieldHeight)
                        allowRotate = false;

                    // Doesn't allow rotations of the figure if there are any other blocks
                    if (Figure.figures[figure.num, rot, j, i] == 1 && field[figure.x + i, figure.y + j] && Figure.figures[figure.num, figure.rot, j, i] == 0)
                        allowRotate = false;
                }
            }

            if (allowRotate)
            {
                DestroyFigure();
                figure.Rotate();
                CreateFigure();
                UpdateFigureHelper();
            }
            else
            {
                figure.x = xOld;
            }
        }

        IEnumerator FallRoutine()
        {
            while (!gameOver)
            {
                yield return new WaitForSeconds(figureFallStartDelay / (1.0f + ((level - 1) * figureFallDecreaseCoeff)));
                MoveDown();
            }
        }

        IEnumerator MoveLeftRoutine()
        {
            while (!gameOver && (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)))
            {
                yield return new WaitForSeconds(keyDelay);

                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                    MoveLeft();
            }

            yield return null;
        }

        IEnumerator MoveRightRoutine()
        {
            while (!gameOver && (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)))
            {
                yield return new WaitForSeconds(keyDelay);
                
                if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                    MoveRight();
            }

            yield return null;
        }

        IEnumerator MoveDownRoutine()
        {
            while (!gameOver && (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)))
            {
                yield return new WaitForSeconds(keyDelay);

                if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                {
                    MoveDown();
                    StopCoroutine("FallRoutine");
                    StartCoroutine("FallRoutine");
                }
            }

            yield return null;
        }

        IEnumerator TakeScreenshot()
        {
            yield return new WaitForEndOfFrame();

            if (!System.IO.Directory.Exists("Screenshots"))
                System.IO.Directory.CreateDirectory("Screenshots");

            for (int i = 0; ; i++)
            {
                if (!System.IO.File.Exists("Screenshots/Screenshot" + Convert.ToString(i) + ".png"))
                {
                    ScreenCapture.CaptureScreenshot(System.IO.Directory.GetCurrentDirectory() + "/Screenshots/Screenshot" + Convert.ToString(i) + ".png");
                    break;
                }
            }
        }

        void SaveSettings()
        {
            PlayerPrefs.SetInt("SoundEnabled", Convert.ToInt32(soundEnabled));
            PlayerPrefs.SetInt("FigureHelperEnabled", Convert.ToInt32(figureHelperEnabled));
            PlayerPrefs.SetInt("Record", score);
            PlayerPrefs.Save();
        }

        void Start()
        {
            if (PlayerPrefs.HasKey("SoundEnabled")) soundEnabled = Convert.ToBoolean(PlayerPrefs.GetInt("SoundEnabled"));
            if (PlayerPrefs.HasKey("FigureHelperEnabled")) figureHelperEnabled = Convert.ToBoolean(PlayerPrefs.GetInt("FigureHelperEnabled"));

            if (PlayerPrefs.HasKey("Record"))
            {
                record = PlayerPrefs.GetInt("Record");
                textRecordCount.text = Convert.ToString(record);
            }

            AudioListener.volume = soundEnabled ? 1.0f : 0.0f;
            NewGame();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
            
            // Show help panel
            if (Input.GetKeyDown(KeyCode.F1)) panelHelp.SetActive(!panelHelp.activeSelf);

            // Take a screenshot
            if (Input.GetKeyDown(KeyCode.F12)) StartCoroutine(TakeScreenshot());

            // Turn on/off sound
            if (Input.GetKeyDown(KeyCode.T))
            {
                soundEnabled = !soundEnabled;
                AudioListener.volume = soundEnabled ? 1.0f : 0.0f;
                SaveSettings();
            }

            // Turn on/off the helper
            if (Input.GetKeyDown(KeyCode.H))
            {
                figureHelperEnabled = !figureHelperEnabled;
                UpdateFigureHelper();
                SaveSettings();
            }

            // Game logic
            if (!gameOver)
            {
                // Moves the figure left
                if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    MoveLeft();
                    StopCoroutine("MoveLeftRoutine");
                    StartCoroutine("MoveLeftRoutine");
                }
                // Moves the figure right
                if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                {
                    MoveRight();
                    StopCoroutine("MoveRightRoutine");
                    StartCoroutine("MoveRightRoutine");
                }
                // Moves the figure down
                if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
                {
                    MoveDown();
                    StopCoroutine("MoveDownRoutine");
                    StartCoroutine("MoveDownRoutine");
                    StopCoroutine("FallRoutine");
                    StartCoroutine("FallRoutine");
                }
                // Rotate the figure
                if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                {
                    RotateFigure();
                }
                // Drop the figure
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    DropFigure();
                }

                // Creates a new figure when the current figure is dropped
                if (IsFigureDropped())
                {
                    AddFigureIntoField();
                    RemoveFilledLines();
                    if (soundDrop) AudioSource.PlayClipAtPoint(soundDrop, new Vector3());

                    figure.New((fieldWidth / 2) - (Figure.width / 2), 0);

                    if (!IsFigureDropped() && IsThereFreeSpaceForNewFigure())
                    {
                        UpdateNextFigure();
                        CreateFigure();
                        UpdateFigureHelper();
                    }

                    if (IsFigureDropped() || !IsThereFreeSpaceForNewFigure())
                    {
                        DestroyFigure();
                        gameOver = true;
                        PanelGameOver.SetActive(true);
                        StopAllCoroutines();
                    }
                }
            }
            else
            {
                // Save a new record when the game is ended
                if (score > record)
                {
                    record = score;
                    textRecordCount.text = Convert.ToString(record);
                    SaveSettings();
                }

                // Starts a new game
                if (Input.GetKeyDown(KeyCode.Space)) NewGame();
            }
        }
    }
}