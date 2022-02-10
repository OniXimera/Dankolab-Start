using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using System;

public class PlayfieldGeneration : MonoBehaviour
{
    [SerializeField] GameObject _prefab;
    [SerializeField] TMP_InputField _widthInput;
    [SerializeField] TMP_InputField _heightInput;
    [SerializeField] TMP_InputField _colorInput;
    [SerializeField] Color[] _colorPalit;
    [SerializeField] TMP_Text _scoreText;
    [SerializeField] TMP_Text _moveText;
    [SerializeField] Message _messageText;

    private GameObject[,] _virtualField;
    private List<int[]> _blockCoincidencesColor;
    private int[,] _directionssearch = new int[,] { { 0, -1 }, { 0, 1 }, { 1, 0 }, { -1, 0 } };
    private Vector3 _startPosition;
    private Vector3 _position;
    private Color _searchColor;
    
    private int _width, _height, _color, _score, _move;
    private float _cube;

    private void Start()
    {
        CreatePlayingField();
    }
    private void OnDisable()
    {
        foreach (Button child in GetComponentsInChildren<Button>())
            child.gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
    }
    public void CreatePlayingField()
    {
        if (int.TryParse(_widthInput.text, out _width)) //Проверка вводных.
            if (_width < 10 || _width > 50)
            { 
                _messageText.ShowMessage("Ошибка в поле ширина! Значенье должно быть числом не меньше 10 и не больше 50.");
                _widthInput.text = "16";
                return;
            }
        if (int.TryParse(_heightInput.text, out _height))
            if (_height < 10 || _height > 50)
            {
                _messageText.ShowMessage("Ошибка в поле высота! Значенье должно быть числом не меньше 10 и не больше 50.");
                _heightInput.text = "10";
                return;
            } 
        if (int.TryParse(_colorInput.text, out _color))
            if (_color < 2 || _color > 5)
            {
                _messageText.ShowMessage("Ошибка в поле цвет! Значенье должно быть числом не меньше 2 и не больше 5.");
                _colorInput.text = "3";
                return;
            }

        _move = _score = 0;
        
        float widthPole = GetComponent<RectTransform>().rect.width; //Вычисляем максимальный размер блока.
        float heightPole = GetComponent<RectTransform>().rect.height;
        if (widthPole / _width < heightPole / _height)
            _cube = widthPole / _width;
        else
            _cube = heightPole / _height;

        _startPosition = new Vector2(_cube * (_width - 1) / -2, _cube * (_height - 1) / 2);
        _virtualField = new GameObject[_width, _height];

        foreach (Button child in GetComponentsInChildren<Button>()) //Отчистка поля если необходимо.
        {
            child.gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
            DestroyImmediate(child.gameObject);
        }

        for (int w = 0; w < _width; w++) //Создание блоков для поля.
        {
            for (int h = 0; h < _height; h++)
            {
                _virtualField[w, h] = Instantiate(_prefab, transform);
                _virtualField[w, h].GetComponent<RectTransform>().sizeDelta = new Vector2(_cube, _cube);
                _virtualField[w, h].GetComponent<Image>().color = _colorPalit[UnityEngine.Random.Range(0, _color)];
            }
        }

        GameFieldRendering(); //Отрисовка поля.
    }

    private void GameFieldRendering()
    {
        for (int w = 0; w < _width; w++) //Отрисовывает поле согласно виртуальной версии.
        {
            for (int h = 0; h < _height; h++)
            {
                _position = _startPosition + new Vector3(_cube * w, -_cube * h);
                if (_virtualField[w, h] != null && _virtualField[w, h].GetComponent<RectTransform>().localPosition != _position)
                {
                    _virtualField[w, h].GetComponent<RectTransform>().localPosition = _position;
                    int tempW = w, tempH = h;
                    _virtualField[w, h].GetComponent<Button>().onClick.RemoveAllListeners();
                    _virtualField[w, h].GetComponent<Button>().onClick.AddListener(() => TaskOnClick(tempW, tempH));
                }
            }
        }
    }

    private bool CheckForMove()
    {
        for (int w = 0; w < _width; w++)
        {
            for (int h = _height - 1; h > -1; h--)
            {
                if (_virtualField[w, h] == null)
                {
                    break;
                }
                else
                {
                    _blockCoincidencesColor = new List<int[]>() { new int[] { w, h } };
                    SearchColorMatching(_blockCoincidencesColor, true);
                    if (_blockCoincidencesColor.Count > 2)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private void SearchColorMatching(List<int[]> coincidences, bool fast)
    {
        _searchColor = _virtualField[coincidences[0][0], coincidences[0][1]].GetComponent<Image>().color;

        for (int i = 0; i < coincidences.Count; i++)
        {
            for (int j = 0, x, y, directionslength = _directionssearch.GetUpperBound(0) + 1; j < directionslength; j++)
            {
                x = coincidences[i][0] + _directionssearch[j, 0];
                y = coincidences[i][1] + _directionssearch[j, 1];
                if (x > -1 && y > -1 && x < _width && y < _height)
                    if (_virtualField[x, y] != null)
                        if (_searchColor == _virtualField[x, y].GetComponent<Image>().color)
                            if (SearchMatches(coincidences, x, y))
                            { 
                                coincidences.Add(new int[] { x, y });
                                if (fast && coincidences.Count > 2)
                                    return; 
                            }
            }
        }
    }

    private bool SearchMatches(List<int[]> list, int w, int h)
    {
        foreach (int[] item in list)
            if (item[0] == w && item[1] == h)
                return false; 
        return true;
    }

    public void TaskOnClick(int w, int h)
    {
        _blockCoincidencesColor = new List<int[]>() { new int[] { w, h } };

        SearchColorMatching(_blockCoincidencesColor, false);

        if (_blockCoincidencesColor.Count > 2)
        {
            foreach (int[] item in _blockCoincidencesColor)
            {
                Destroy(_virtualField[item[0], item[1]]);
                _virtualField[item[0], item[1]] = null;
            }

            SettleBlocks(_blockCoincidencesColor);

            _move++;
            _score += _blockCoincidencesColor.Count * 2 - 3;
            UpdateScoreAndMove();
        }
        if (!CheckForMove()) //Проверка на наличие ходов.
            _messageText.ShowMessage("Игра окончена! Вы заработали " + _score + " очков!");
    }

    private void SettleBlocks(List<int[]> list)
    {
        List<int> temp = new List<int>(); //Получаем измененьемые столбцы.
        foreach (int[] item in list)
        {
            if (temp.IndexOf(item[0]) == -1)
            {
                temp.Add(item[0]);
            }
        }

        int? firstEmpty;
        foreach (int item in temp) //Смещаем вниз блоки.
        {
            firstEmpty = null;
            for (int i = _height - 1; i > -1; i--)
            {
                if (_virtualField[item, i] == null && !firstEmpty.HasValue)
                {
                    firstEmpty = i;
                }
                else if (firstEmpty.HasValue && _virtualField[item, i] != null)
                {
                    _virtualField[item, firstEmpty.Value] = _virtualField[item, i];
                    _virtualField[item, i] = null;
                    firstEmpty--;
                }
            }
        }

        GameFieldRendering();
    }

    private void UpdateScoreAndMove()//Обновление счёта.
    {
        _scoreText.text = _score.ToString();
        _moveText.text = _move.ToString();
    }
}
