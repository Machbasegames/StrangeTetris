using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block
{
    public Transform transform;
    public SpriteRenderer spriteRenderer;

    public Block(Transform _transform, int _indexY, int _indexX, int _matrixSizeY, int _matrixSizeX) {
        transform = _transform;

        float _basePositionY = transform.localScale.y * (_matrixSizeY / 2);
        float _basePositionX = -(transform.localScale.x * (_matrixSizeX / 2));

        float _offsetY = _indexY * transform.localScale.y;
        float _offsetX = _indexX * transform.localScale.x;

        transform.position = new Vector3(_basePositionX + _offsetX,
                                         _basePositionY - _offsetY,
                                         0f);

        spriteRenderer = transform.GetComponentInChildren<SpriteRenderer>();
        SetSpriteEnable(false);
    }

    public void SetSpriteEnable(bool _isSpriteEnable) {
        //spriteRenderer.enabled = _isSpriteEnable;
        
        Color _color = spriteRenderer.color;

        if(_isSpriteEnable == true) {
            _color.a = 1f;
        } else {
            _color.a = 0.5f;
        }

        spriteRenderer.color = _color;
    }
}

public class MatrixDrawer : MonoBehaviour
{
    public GameObject blockPrefab;

    public int matrixSizeY = 10;
    public int matrixSizeX = 10;

    Block[,] blockMatrix;
    int[,] matrix;

    private void Awake() {
        blockMatrix = new Block[matrixSizeY, matrixSizeX];
        matrix = new int[matrixSizeY, matrixSizeX];

        int _indexY = 0;
        int _indexX = 0;

        for(int i = 0; i < matrix.Length; i++) {
            _indexY = i / matrixSizeY;
            _indexX = i % matrixSizeX;

            Transform _newBlock = GameObject.Instantiate<GameObject>(blockPrefab,
                                                                     Vector3.zero,
                                                                     Quaternion.identity,
                                                                     transform).transform;

            blockMatrix[_indexY, _indexX] = new Block(_newBlock,
                                                      _indexY,
                                                      _indexX,
                                                      matrixSizeY,
                                                      matrixSizeX);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(AutoGenerator());
        StartCoroutine(MatrixProcess());
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator AutoGenerator() {
        while(true) {
            yield return new WaitForSeconds(1f);

            for(int i = 0; i < Random.Range(1, 4); i++) {
                matrix[0, Random.Range(0, matrixSizeX)] = 1;
            }
        }
    }

    void DrawMatrix() {
        for(int y = 0; y < matrixSizeY; y++) {
            for(int x = 0; x < matrixSizeX; x++) {
                blockMatrix[y, x].SetSpriteEnable(false);

                if(matrix[y, x] == 1) {
                    blockMatrix[y, x].SetSpriteEnable(true);
                }
            }
        }
    }

    void DownRow() {
        for(int y = matrixSizeY - 1; y > 0; y--) {
            for(int x = 0; x < matrixSizeX; x++) {
                if(matrix[y, x] != 1) {
                    matrix[y, x] = matrix[y-1, x];
                    matrix[y-1, x] = 0;
                }
            }
        }
    }

    void CalculateMatrix() {
        bool _isFull;
        for(int y = matrixSizeY - 1; y >= 0; y--) {
            _isFull = true;

            for(int x = 0; x < matrixSizeX; x++) {
                if(matrix[y, x] != 1) {
                    _isFull = false;
                    break;
                }
            }

            if(_isFull == true) {
                for(int x = 0; x < matrixSizeX; x++) {
                    matrix[y, x] = 0;
                }
            }
        }
    }

    IEnumerator MatrixProcess() {
        while(true) {
            DrawMatrix();
            DownRow();
            CalculateMatrix();

            yield return new WaitForSeconds(1f);
        }
    }
}
