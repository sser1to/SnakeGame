using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using static Unity.VisualScripting.StickyNote;

public class Snake : MonoBehaviour
{
    private Vector2 direction = Vector2.zero; // ���������, ��� ���������� ������ �� � ��������

    private List<Transform> snaketail; // ������ ������
    [SerializeField] Transform snaketail_prefab; // ������ ������

    // �������� ������ ��� ������ ������
    [SerializeField] Color colorStart = Color.green;
    [SerializeField] Color colorEnd = Color.blue;

    [SerializeField] float startSpeed = 1f; // ��������� �������� ������
    [SerializeField] float boost = 0.02f; // ���� � �������� ������ ��� �������� ������

    [SerializeField] Text pauseText; // ����� � ������
    private bool isPaused = true; // ��������� �����
    private float lastSpeed; // ��������� �������� ����� ������ 

    [SerializeField] int points = 100; // ���-�� ������������ ����� �� 1 ������
    [SerializeField] Text scoreText; // ����� ��� ������ �������� ���������� �����
    private int score = 0; // ������� ���������� �����
    [SerializeField] Text bestScoreText; // ����� ��� ������ ���������� ���������� �����
    private int bestScore = 0; // ��������� ���������� �����

    [SerializeField] AudioClip gameMusic; // ����������� ����
    [SerializeField] AudioClip gameOverSound; // ���� ��������� ����
    [SerializeField] AudioClip beatRecordSound; // ���� ������� �������

    // ���������� ��� ������ �� �������
    private AudioSource gameMusicaudioSource;
    private AudioSource gameOverAudioSource;
    private AudioSource beatRecordAudioSource;

    /// <summary>
    /// ������, � ������� ���������� ������� ���� ������ ������
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
    /// ������� ����������
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
    /// ����������� ������������� ����������� ���� (����� ������ ��������)
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
        snaketail.Add(this.transform); // ��������� ������ � ������ ������
        AddTail(); // ��������� 1 ��. ������

        TogglePause();

        Time.timeScale = startSpeed; // ��������� ��������� ��������

        // ���� ������ ��� ����, �� ����� ������������� �������
        if (PlayerPrefs.HasKey("snakeScore"))
        {
            bestScore = PlayerPrefs.GetInt("snakeScore");
        }
        else
        {
            bestScore = 0;
        }

        CheckScore();

        // ��������������� ����������� ����
        gameMusicaudioSource = gameObject.AddComponent<AudioSource>();
        gameMusicaudioSource.clip = gameMusic;
        gameMusicaudioSource.loop = true;
        gameMusicaudioSource.Play();
    }

    /// <summary>
    /// ����� �����
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
    /// ���������� ������
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
    /// ������������ ������
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
            // ��������������� ����� ��������� ����
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

        // ���� ������� ���������� ����� ������, ��� ������, �� ��������� ������ � �������� ��� � ����� PlayerPrefs
        if (score >= bestScore)
        {
            bestScore = score;
            PlayerPrefs.SetInt("snakeScore", bestScore);
        }

        Time.timeScale = startSpeed;

        // ��������� �������� ���������� �����
        score = 0;
        CheckScore();

        direction = Vector2.zero;

        // ��������� ��������������� ����������� ����
        if (gameMusicaudioSource != null)
        {
            gameMusicaudioSource.Pause();
        }
    }

    /// <summary>
    /// ������������ ��������� �����
    /// </summary>
    private void TogglePause()
    {
        if (!isPaused)
        {
            pauseText.text = "PAUSE";
            lastSpeed = Time.timeScale;
            Time.timeScale = 0;

            // ��������� ��������������� ����������� ����
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

            // ������������� ��������������� ����������� ����
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
            score += points; // ���������� ����
            CheckScore();
            Time.timeScale += boost; // ���������� ��������

            // ���� ������ �����, �� ��������������� ����� ������� �������
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
