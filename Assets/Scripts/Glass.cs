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

namespace IlyaLts.Tetris
{
    public class Glass : MonoBehaviour
    {
        const int numberOfLevels = 15;
        const int scoreFor1Line = 10;
        const int scoreFor2Lines = 30;
        const int scoreFor3Lines = 60;
        const int scoreFor4Lines = 100;
        const int startupScoreGoal = 100;
        const int scoreGoalMultiplier = 2;
        const int maxScore = 999999;
        const int glassWidth = 10;
        const int glassHeight = 20;
        const float keyDelay = 0.15f;
        const float figureFallDelay = 1.0f;
        const float figureFallDelayDecrease = 0.05f;
        const float figureHelperAlpha = 0.33f;

        public Text textScoreCount;
        public Text textGoalCount;
        public Text textLevelCount;
        public Text textRecordCount;
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

        Block[,] glass = new Block[glassWidth, glassHeight];
        Figure figure = new Figure();
        GameObject[,] figureNext = new GameObject[Figure.width, Figure.height];
        GameObject[,] figureHelper = new GameObject[Figure.width, Figure.height];

        void CreateBlock(ref GameObject obj, Vector3 pos, int color, String parentName, float alpha = 1.0f)
        {
            obj = Instantiate(block, pos, Quaternion.identity);
            obj.GetComponent<Image>().sprite = blocks[color];
            obj.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, alpha);
            obj.transform.SetParent(GameObject.Find(parentName).transform, false);
        }

        void DestroyBlock(ref GameObject obj)
        {
            if (obj)
            {
                Destroy(obj);
                obj = null;
            }
        }

        void CreateFigure()
        {
            for (int i = 0; i < Figure.width; i++)
            {
                for (int j = 0; j < Figure.height; j++)
                {
                    if (figure.blocks[i, j].filled)
                    {
                        CreateBlock(ref figure.blocks[i, j].obj, new Vector3(Block.width * (i + figure.x), -Block.height * (j + figure.y), 0.0f), figure.num, "Glass");
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
                    if (figure.blocks[i, j].filled)
                    {
                        DestroyBlock(ref figure.blocks[i, j].obj);
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

                    if (Figure.figures[figure.numNext, 0, i, j] == 1)
                    {
                        CreateBlock(ref figureNext[i, j], new Vector3(Block.width * j, -Block.height * i, 0.0f), figure.numNext, "PanelNextFigure");
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

                    // Don't allow to create a figure helper when the current figure is too low
                    if (figure.blocks[i, j].filled && figureHelperEnabled && tempY + Figure.height < figure.y)
                    {
                        CreateBlock(ref figureHelper[i, j], new Vector3(Block.width * (i + figure.x), -Block.height * (j + figure.y), 0.0f), figure.num, "Glass", figureHelperAlpha);
                    }
                }
            }

			figure.y = tempY;
        }

        void NewGame()
        {
            for (int i = 0; i < glassWidth; i++)
            {
                for (int j = 0; j < glassHeight; j++)
                {
                    glass[i, j].filled = false;
                    DestroyBlock(ref glass[i, j].obj);
                }
            }

            DestroyFigure();
            figure.New((glassWidth / 2) - (Figure.width / 2), 0);
            UpdateNextFigure();
            CreateFigure();
            UpdateFigureHelper();

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

        void BuildFigureIntoGlass()
        {
            for (int i = 0; i < Figure.width; i++)
            {
                for (int j = 0; j < Figure.height; j++)
                {
                    if (figure.blocks[i, j].filled)
                    {
                        glass[figure.x + i, figure.y + j].filled = true;
                        glass[figure.x + i, figure.y + j].clr = figure.blocks[i, j].clr;
                        glass[figure.x + i, figure.y + j].obj = figure.blocks[i, j].obj;
                    }
                }
            }
        }

        void RemoveFilledLines()
        {
            int numOfFilledLines = 0;

            for (int i = glassHeight - 1; i > 0; i--)
            {
                bool filled = true;

                for (int j = 0; j < glassWidth; j++)
                {
                    if (!glass[j, i].filled)
                    {
                        filled = false;
                    }
                }

                if (filled)
                {
                    // Destroy filled lines
                    for (int j = 0; j < glassWidth; j++)
                    {
                        glass[j, i].filled = false;
                        DestroyBlock(ref glass[j, i].obj);
                    }

                    // Move upper lines down
                    for (int len = i; len > 0; len--)
                    {
                        for (int j = 0; j < glassWidth; j++)
                        {   
                            glass[j, len].filled = glass[j, len-1].filled;
                            glass[j, len].clr = glass[j, len-1].clr;
                            glass[j, len-1].filled = false;

                            glass[j, len].obj = glass[j, len-1].obj;
                            glass[j, len-1].obj = null;

                            if (glass[j, len].obj)
                            {
                                Transform vec = glass[j, len].obj.GetComponent<Transform>();
                                vec.position = new Vector3(vec.position.x, vec.position.y - Block.height, 0.0f);
                            }
                        }
                    }

                    i++;
                    numOfFilledLines++;
                }
            }

            if (Convert.ToBoolean(numOfFilledLines))
            {
                if (numOfFilledLines == 1)
                    score += scoreFor1Line * level;
                else if (numOfFilledLines == 2)
                    score += scoreFor2Lines * level;
                else if (numOfFilledLines == 3)
                    score += scoreFor3Lines * level;
                else
                    score += scoreFor4Lines * level;

                if (score >= scoreGoal)
                {
                    if (level == numberOfLevels)
                    {
                        score = maxScore;
                        scoreGoal = maxScore;
                        gameOver = true;
                        PanelGameOver.SetActive(true);
                        return;
                    }

                    level += 1;
                    scoreGoal *= scoreGoalMultiplier;
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
                    if (figure.blocks[i, j].filled)
                    {
                        if (j+1 < Figure.height && figure.blocks[i, j + 1].filled && glass[figure.x + i, figure.y + j + 1].filled)
                            continue;

                        if (figure.y + j + 1 == glassHeight || glass[figure.x + i, figure.y + j + 1].filled)
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
                    if (figure.blocks[i, j].filled && glass[figure.x + i, figure.y + j].filled)
                        return false;

            return true;
        }

        public void MoveLeft()
        {
            for (int i = 0; i < Figure.width; i++)
            {
                for (int j = 0; j < Figure.height; j++)
                {
                    if (figure.x + i <= 0 && figure.blocks[i, j].filled)
                        return;

                    if (figure.blocks[i, j].filled && glass[figure.x + i - 1, figure.y + j].filled)
                        return;
                }
            }

            figure.Move(-1, 0);
        }

        public void MoveRight()
        {
            for (int i = Figure.width - 1; i >= 0; i--)
            {
                for (int j = 0; j < Figure.height; j++)
                {
                    if (figure.x + i >= glassWidth - 1 && figure.blocks[i, j].filled)
                        return;

                    if (figure.blocks[i, j].filled && glass[figure.x + i + 1, figure.y + j].filled)
                        return;
                }
            }

            figure.Move(1, 0);
        }

        public void MoveDown()
        {
            if (!IsFigureDropped())
            {
                figure.Move(0, -1);
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

            // Move the figure right from wall if there is not enough free space to rotate
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

            // Move the figure left from wall if there is not enough free space to rotate
            do
            {
                flag = true;

                for (int i = Figure.width - 1; i > glassWidth - 1 - figure.x; i--)
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

            // Don't allow to rotate the figure when it's too low
            for (int i = Figure.height - 1; i >= 0; i--)
                for (int j = 0; j < Figure.width; j++)
                    if (Figure.figures[figure.num, rot, j, i] == 1 && figure.y + j >= glassHeight)
                        allowRotate = false;

            // Don't allow to rotate the figure if there are any other blocks
            for (int i = 0; i < Figure.width; i++)
                for (int j = 0; j < Figure.height; j++)
                    if (Figure.figures[figure.num, rot, j, i] == 1 && glass[figure.x + i, figure.y + j].filled && Figure.figures[figure.num, figure.rot, j, i] == 0)
                        allowRotate = false;

            if (allowRotate)
            {
                DestroyFigure();
                figure.Rotate();
                CreateFigure();
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
                yield return new WaitForSeconds(figureFallDelay - figureFallDelayDecrease * level);
                MoveDown();
                UpdateFigureHelper();
            }
        }

        IEnumerator MoveLeftRoutine()
        {
            while (!gameOver)
            {
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                {
                    MoveLeft();
                    UpdateFigureHelper();
                    yield return new WaitForSeconds(keyDelay);
                }

                yield return null;
            }
        }

        IEnumerator MoveRightRoutine()
        {
            while (!gameOver)
            {
                if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                {
                    MoveRight();
                    UpdateFigureHelper();
                    yield return new WaitForSeconds(keyDelay);
                }

                yield return null;
            }
        }

        IEnumerator MoveDownRoutine()
        {
            while (!gameOver)
            {
                if ((Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)))
                {
                    MoveDown();
                    UpdateFigureHelper();

                    StopCoroutine("FallRoutine");
                    StartCoroutine("FallRoutine");

                    yield return new WaitForSeconds(keyDelay);
                }

                yield return null;
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

            if (soundEnabled)
                AudioListener.volume = 1.0f;
            else
                AudioListener.volume = 0.0f;

            NewGame();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();

            // Show help panel
            if (Input.GetKeyDown(KeyCode.F1)) panelHelp.SetActive(!panelHelp.activeSelf);

            // Take a screenshot
            if (Input.GetKeyDown(KeyCode.F12))
            {
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

            // Turn on/off sound
            if (Input.GetKeyDown(KeyCode.T))
            {
                soundEnabled = !soundEnabled;

                if (soundEnabled)
                    AudioListener.volume = 1.0f;
                else
                    AudioListener.volume = 0.0f;

                SaveSettings();
            }

            // Turn on/off the helper
            if (Input.GetKeyDown(KeyCode.H))
            {
                figureHelperEnabled = !figureHelperEnabled;
                SaveSettings();
                UpdateFigureHelper();
            }

            // Game logic
            if (!gameOver)
            {
                // Rotate the figure
                if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                {
                    RotateFigure();
                    UpdateFigureHelper();
                }
                // Drop the figure
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    DropFigure();
                    UpdateFigureHelper();
                }

                // Creates a new figure when the current figure is dropped
                if (IsFigureDropped())
                {
                    BuildFigureIntoGlass();
                    RemoveFilledLines();
                    if (soundDrop) AudioSource.PlayClipAtPoint(soundDrop, new Vector3());

                    figure.New((glassWidth / 2) - (Figure.width / 2), 0);

                    if (!IsFigureDropped())
                    {
                        UpdateNextFigure();
                        CreateFigure();
                        UpdateFigureHelper();
                    }

                    if (IsFigureDropped() || !IsThereFreeSpaceForNewFigure())
                    {
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
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    NewGame();
                    PanelGameOver.SetActive(false);
                }
            }
        }
    }
}