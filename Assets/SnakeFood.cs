using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeFood : MonoBehaviour
{
    [SerializeField] BoxCollider2D SpawnArea; // ����� ��� ���������

    private Snake snake; // ������ Snake

    [SerializeField] AudioClip eatAppleSound; // ���� �������� ������ 
    private AudioSource eatAppleAudioSource;

    private void Start()
    {
        StartCoroutine(InitializeSnake());
    }

    // ������������� ������� Snake
    private IEnumerator InitializeSnake()
    {
        yield return new WaitUntil(() => GameObject.FindObjectOfType<Snake>() != null);
        snake = GameObject.FindObjectOfType<Snake>();
        RandomPosition();
    }

    /// <summary>
    /// ��������� ��������� ������
    /// </summary>
    private void RandomPosition()
    {
        Bounds bounds = this.SpawnArea.bounds;
        Vector3 newPosition;
        bool isValidPosition;

        do
        {
            float x = Random.Range(bounds.min.x, bounds.max.x);
            float y = Random.Range(bounds.min.y, bounds.max.y);
            newPosition = new Vector3(Mathf.Round(x), Mathf.Round(y), 0f);
            isValidPosition = true;

            foreach (var snakePosition in snake.SnakePositions)
            {
                if (newPosition == snakePosition)
                {
                    isValidPosition = false;
                    break;
                }
            }
        }
        while (!isValidPosition);

        this.transform.position = newPosition;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // ��� ��������������� �� ������� ������ �����������
        if (other.tag == "snake")
        {
            // ��������������� ����� �������� ������
            eatAppleAudioSource = gameObject.AddComponent<AudioSource>();
            eatAppleAudioSource.clip = eatAppleSound;
            eatAppleAudioSource.Play();

            RandomPosition();
        }
    }
}
