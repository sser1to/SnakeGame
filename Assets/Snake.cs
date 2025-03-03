using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using static Unity.VisualScripting.StickyNote;

public class Snake : MonoBehaviour
{
    private Vector2 direction = Vector2.zero; // Указываем, что изначально объект не в движении

    private List<Transform> snaketail; // Состав змейки
    [SerializeField] Transform snaketail_prefab; // Префаб хвоста

    // Диапазон цветов для хвоста змейки
    [SerializeField] Color colorStart = Color.green;
    [SerializeField] Color colorEnd = Color.blue;

    [SerializeField] float startSpeed = 1f; // Начальная скорость змейки
    [SerializeField] float boost = 0.02f; // Буст к скорости змейки при съедении яблока

    [SerializeField] Text pauseText; // Текст с паузой
    private bool isPaused = true; // Состояние паузы
    private float lastSpeed; // Последняя скорость перед паузой 

    [SerializeField] int points = 100; // Кол-во прибавляемых очков за 1 яблоко
    [SerializeField] Text scoreText; // Текст для вывода текущего количества очков
    private int score = 0; // Текущее количество очков
    [SerializeField] Text bestScoreText; // Текст для вывода рекордного количества очков
    private int bestScore = 0; // Рекордное количество очков

    [SerializeField] AudioClip gameMusic; // Музыкальная тема
    [SerializeField] AudioClip gameOverSound; // Звук окончания игры
    [SerializeField] AudioClip beatRecordSound; // Звук побития рекорда

    // Компоненты для работы со звуками
    private AudioSource gameMusicaudioSource;
    private AudioSource gameOverAudioSource;
    private AudioSource beatRecordAudioSource;

    /// <summary>
    /// Список, в котором содержатся позиции всех частей змейки
    /// </summary>
    public List<Vector3> SnakePositions
    {
        get
        {
            List<Vector3> positions = new List<Vector3>();
            foreach (var segment in snaketail)
            {
                positions.Add(segment.position);
            }
            return positions;
        }
    }

    /// <summary>
    /// Игровое управление
    /// </summary>
    private void Update()
    {
        if (!isPaused)
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                if (direction == Vector2.down)
                {
                    return;
                }
                direction = Vector2.up;
                ResumeMusic();
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                if (direction == Vector2.up)
                {
                    return;
                }
                direction = Vector2.down;
                ResumeMusic();
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                if (direction == Vector2.right)
                {
                    return;
                }
                direction = Vector2.left;
                ResumeMusic();
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                if (direction == Vector2.left)
                {
                    return;
                }
                direction = Vector2.right;
                ResumeMusic();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
        }
    }

    /// <summary>
    /// Продолжение вопроизвдения музыкальной темы (после начала движения)
    /// </summary>
    private void ResumeMusic()
    {
        if (gameMusicaudioSource != null && !gameMusicaudioSource.isPlaying && !isPaused)
        {
            gameMusicaudioSource.UnPause();
        }
    }

    private void Start()
    {
        snaketail = new List<Transform>();
        snaketail.Add(this.transform); // Добавляем голову в состав змейки
        AddTail(); // Добавляем 1 ед. хвоста

        TogglePause();

        Time.timeScale = startSpeed; // Установка начальной скорости

        // Если рекорд уже есть, то вывод существующего рекорда
        if (PlayerPrefs.HasKey("snakeScore"))
        {
            bestScore = PlayerPrefs.GetInt("snakeScore");
        }
        else
        {
            bestScore = 0;
        }

        CheckScore();

        // Воспроизведение музыкальной темы
        gameMusicaudioSource = gameObject.AddComponent<AudioSource>();
        gameMusicaudioSource.clip = gameMusic;
        gameMusicaudioSource.loop = true;
        gameMusicaudioSource.Play();
    }

    /// <summary>
    /// Вывод очков
    /// </summary>
    private void CheckScore()
    {
        if (score >= bestScore)
        {
            bestScore = score;
        }

        scoreText.text = score.ToString("D10");
        bestScoreText.text = bestScore.ToString("D10");
    }

    /// <summary>
    /// Добавление хвоста
    /// </summary>
    private void AddTail()
    {
        Transform tail = Instantiate(this.snaketail_prefab);
        tail.position = snaketail[snaketail.Count - 1].position;

        Color randomColor = Color.Lerp(colorStart, colorEnd, Random.Range(0f, 1f));
        tail.GetComponent<SpriteRenderer>().color = randomColor;

        snaketail.Add(tail);
    }

    /// <summary>
    /// Передвижение змейки
    /// </summary>
    private void FixedUpdate()
    {
        for (int i = snaketail.Count - 1; i > 0; i--)
        {
            snaketail[i].position = snaketail[i - 1].position;
        }

        this.transform.position = new Vector3(Mathf.Round(this.transform.position.x) + direction.x, Mathf.Round(this.transform.position.y) + direction.y, 0f);
    }

    private void GameOver()
    {
        if (direction != Vector2.zero)
        {
            // Воспроизведение звука окончания игры
            gameOverAudioSource = gameObject.AddComponent<AudioSource>();
            gameOverAudioSource.clip = gameOverSound;
            gameOverAudioSource.Play();
        }

        for (int i = 1; i < snaketail.Count; i++)
        {
            Destroy(snaketail[i].gameObject);
        }

        snaketail.Clear();
        snaketail.Add(this.transform);

        this.transform.position = Vector3.zero;
        AddTail();

        // Если текущее количество очков больше, чем рекорд, то обновляем рекорд и сохрняем его в класс PlayerPrefs
        if (score >= bestScore)
        {
            bestScore = score;
            PlayerPrefs.SetInt("snakeScore", bestScore);
        }

        Time.timeScale = startSpeed;

        // Обнуление текущего количества очков
        score = 0;
        CheckScore();

        direction = Vector2.zero;

        // Остановка воспроизведения музыкальной темы
        if (gameMusicaudioSource != null)
        {
            gameMusicaudioSource.Pause();
        }
    }

    /// <summary>
    /// Перелюкчение состояния паузы
    /// </summary>
    private void TogglePause()
    {
        if (!isPaused)
        {
            pauseText.text = "PAUSE";
            lastSpeed = Time.timeScale;
            Time.timeScale = 0;

            // Остановка воспроизведения музыкальной темы
            if (gameMusicaudioSource != null)
            {
                gameMusicaudioSource.Pause();
            }

            isPaused = true;
        }
        else
        {
            pauseText.text = "";
            Time.timeScale = lastSpeed;

            // Возобновление воспроизведения музыкальной темы
            if (gameMusicaudioSource != null)
            {
                gameMusicaudioSource.UnPause();
            }

            isPaused = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "fruit")
        {
            AddTail();
            score += points; // Прибавляем очки
            CheckScore();
            Time.timeScale += boost; // Прибавляем скорость

            // Если рекорд побит, то воспроизведение звука побития рекорда
            if (score == PlayerPrefs.GetInt("snakeScore") && PlayerPrefs.GetInt("snakeScore") != 0)
            {
                beatRecordAudioSource = gameObject.AddComponent<AudioSource>();
                beatRecordAudioSource.clip = beatRecordSound;
                beatRecordAudioSource.Play();
            }
        }
        else if (other.tag == "gameover")
        {
            GameOver();
        }
    }
}
