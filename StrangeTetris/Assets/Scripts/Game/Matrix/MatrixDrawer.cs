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
        SetSpriteLevel(0);
    }

    public void SetSpriteLevel(int _spriteLevel) {
        Color _color = spriteRenderer.color;

        if(_spriteLevel == 1) {
            _color.a = 1.0f;
        } else if(_spriteLevel == 2) {
            _color.a = 0.6f;
        } else if(_spriteLevel == 0) {
            _color.a = 0.3f;
        }

        spriteRenderer.color = _color;
    }
}

public class MatrixDrawer : MonoBehaviour
{
    public GameObject blockPrefab;

    public int matrixSizeY = 20;
    public int matrixSizeX = 10;

    Block[,] blockMatrix;
    int[,] matrix;
    int[,] shadowMatrix;
    bool isGenerate;

    int[,,] blockSet = new int[7, 4, 4] {
        { {0, 2, 2, 0},
          {0, 2, 2, 0},
          {0, 0, 0, 0},
          {0, 0, 0, 0} },
        { {0, 2, 0, 0},
          {0, 2, 0, 0},
          {0, 2, 2, 0},
          {0, 0, 0, 0} },
        { {0, 0, 2, 0},
          {0, 2, 2, 0},
          {0, 0, 2, 0},
          {0, 0, 0, 0} },
        { {0, 0, 2, 0},
          {0, 0, 2, 0},
          {0, 2, 2, 0},
          {0, 0, 0, 0} },
        { {0, 2, 2, 0},
          {2, 2, 0, 0},
          {0, 0, 0, 0},
          {0, 0, 0, 0} },
        { {2, 2, 0, 0},
          {0, 2, 2, 0},
          {0, 0, 0, 0},
          {0, 0, 0, 0} },
        { {0, 2, 0, 0},
          {0, 2, 0, 0},
          {0, 2, 0, 0},
          {0, 2, 0, 0} },
    };

    private void Awake() {
        blockMatrix = new Block[matrixSizeY, matrixSizeX];
        matrix = new int[matrixSizeY, matrixSizeX];
        shadowMatrix = new int[matrixSizeY, matrixSizeX];

        for(int y = 0; y < matrixSizeY; y++) {
            for(int x = 0; x < matrixSizeX; x++) {
                Transform _newBlock = GameObject.Instantiate<GameObject>(blockPrefab,
                                                                        Vector3.zero,
                                                                        Quaternion.identity,
                                                                        transform).transform;

                blockMatrix[y, x] = new Block(_newBlock,
                                                        y,
                                                        x,
                                                        matrixSizeY,
                                                        matrixSizeX);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(AutoGenerator());
        StartCoroutine(MatrixProcess());

        isGenerate = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.LeftArrow)) {
            MoveMatrix(-1);
        }
        if(Input.GetKeyDown(KeyCode.RightArrow)) {
            MoveMatrix(1);
        }
        if(Input.GetKeyDown(KeyCode.DownArrow)) {
            DownEnd();
        }

        if(Input.GetKeyDown(KeyCode.Space)) {
            GenerateBlock();
        }
    }

    private void LateUpdate() {
        DrawMatrix();
    }

    public void MoveMatrix(int _direction) {
        int[,] _shadowMatrix = new int[matrixSizeY, matrixSizeX];

        for(int y = 0; y < matrixSizeY; y++) {
            for(int x = 0; x < matrixSizeX; x++) {
                if(shadowMatrix[y, x] == 2) {
                    if(((x + _direction) == matrixSizeX) || ((x + _direction) == -1)) {
                        return ;
                    }
                    _shadowMatrix[y, x + _direction] = shadowMatrix[y, x];
                }
            }
        }

        if(IsCollisionMatrix(ref _shadowMatrix) == true) {
            return ;
        }

        for(int y = 0; y < matrixSizeY; y++) {
            for(int x = 0; x < matrixSizeX; x++) {
                shadowMatrix[y, x] = _shadowMatrix[y, x];
            }
        }
    }

    public void DownEnd() {
        while(isGenerate == false) {
            DownRow();
        }
    }

    void GenerateBlock() {
        int _index = Random.Range(0, 7);
        int _offsetX = Random.Range(0, matrixSizeX-4);

        for(int y = 0; y < matrixSizeY; y++) {
            for(int x = 0; x < matrixSizeX; x++) {
                shadowMatrix[y, x] = 0;
            }
        }

        for(int y = 0; y < 4; y++) {
            for(int x = 0; x < 4; x++) {
                shadowMatrix[y, x + _offsetX] += blockSet[_index, y, x];
            }
        }
    }

    IEnumerator AutoGenerator() {
        while(true) {
            yield return null;

            if(isGenerate == true) {
                yield return new WaitForSeconds(1f);

                GenerateBlock();
                isGenerate = false;
            }
        }
    }

    void DrawMatrix() {
        for(int y = 0; y < matrixSizeY; y++) {
            for(int x = 0; x < matrixSizeX; x++) {
                blockMatrix[y, x].SetSpriteLevel(matrix[y, x]);
                
                if(shadowMatrix[y, x] == 2) {
                    blockMatrix[y, x].SetSpriteLevel(2);
                }
            }
        }
    }

    void FixMatrix() {
        for(int y = 0; y < matrixSizeY; y++) {
            for(int x = 0; x < matrixSizeX; x++) {
                if(shadowMatrix[y, x] == 2) {
                    matrix[y, x] = 1;
                    shadowMatrix[y, x] = 0;
                }
            }
        }

        CalculateMatrix();

        isGenerate = true;
    }

    bool IsCollisionMatrix(ref int[,] _shadowMatrix) {
        for(int y = 0; y < matrixSizeY; y++) {
            for(int x = 0; x < matrixSizeX; x++) {
                if(matrix[y, x] + _shadowMatrix[y, x] == 3) {
                    return true;
                }
            }
        }

        return false;
    }

    void DownRow() {
        int[,] _shadowMatrix = new int[matrixSizeY, matrixSizeX];

        for(int y = 0; y < matrixSizeY; y++) {
            for(int x = 0; x < matrixSizeX; x++) {
                _shadowMatrix[y, x] = shadowMatrix[y, x];
            }
        }

        DownAllRow(ref _shadowMatrix);
        if(IsCollisionMatrix(ref _shadowMatrix) == true) {
            FixMatrix();
            return ;
        }

        DownAllRow(ref shadowMatrix);
        for(int x = 0; x < matrixSizeX; x++) {
            if(shadowMatrix[matrixSizeY - 1, x] != 0) {
                FixMatrix();
                return ;
            }
        }
    }

    void DownAllRow(ref int[,] _matrix) {
        for(int y = matrixSizeY - 1; y > 0; y--) {
            for(int x = 0; x < matrixSizeX; x++) {
                if(_matrix[y, x] == 0) {
                    _matrix[y, x] = _matrix[y-1, x];
                    _matrix[y-1, x] = 0;
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

                DownAllRow(ref matrix);
            }
        }
    }

    IEnumerator MatrixProcess() {
        while(true) {
            DownRow();

            yield return new WaitForSeconds(1f);
        }
    }
}
