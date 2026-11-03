
using UnityEngine;
using System;
using System.Collections.Generic;

namespace Deforestation
{
    public class TreeTerrainController : MonoBehaviour
    {
        #region Properties
        public TreeInstance[] Trees => _trees;
        #endregion

        #region Fields
        [SerializeField] private Tree _treeDetectionPrefab;
        [SerializeField] private Tree _treePrefab;

        private TreeInstance[] _trees;
        private Terrain _terrain;

        // Guarda los indices originales de los arboles eliminados
        private HashSet<int> _removedTreeIndices = new HashSet<int>();

        #endregion

        #region Unity Callbacks

        void Start()
        {
            _terrain = Terrain.activeTerrain;
            _trees = _terrain.terrainData.treeInstances;

            InitializeTrees();
        }

        private void InitializeTrees()
        {
            for (int i = _trees.Length - 1; i >= 0; i--)
            {
                TreeInstance tree = _trees[i];

                Vector3 treeWorldPos = TreeToWorldPosition(tree);

                Tree treeDetector = Instantiate(
                    _treeDetectionPrefab,
                    treeWorldPos,
                    Quaternion.identity
                );

                treeDetector.transform.parent = transform;
                treeDetector.Index = i;
            }
        }

        public GameObject DestroyTree(int i, Vector3 treeWorldPos)
        {
            // Evitar intentar eliminar indices invalidos
            if (_trees == null || i < 0 || i >= _trees.Length)
            {
                Debug.LogWarning("Indice de arbol invalido: " + i);
                return null;
            }

            // Crear el arbol fisico
            Tree newTree = Instantiate(
                _treePrefab,
                treeWorldPos,
                Quaternion.identity
            );

            RemoveTreeFromTerrain(i);

            return newTree.gameObject;
        }

        void OnDestroy()
        {
            if (_terrain != null && _trees != null)
            {
                _terrain.terrainData.treeInstances = _trees;
            }
        }

        #endregion

        #region Public Methods

        public Vector3 TreeToWorldPosition(TreeInstance tree)
        {
            return Vector3.Scale(
                tree.position,
                _terrain.terrainData.size
            ) + _terrain.transform.position;
        }

        public void RemoveTreeFromTerrain(int index)
        {
            if (_terrain == null || _trees == null)
                return;

            // Comprobar que el indice original existe
            if (index < 0 || index >= _trees.Length)
            {
                Debug.LogWarning("Indice fuera de rango: " + index);
                return;
            }

            // Evitar eliminar dos veces el mismo arbol
            if (_removedTreeIndices.Contains(index))
                return;

            List<TreeInstance> trees = new List<TreeInstance>(
                _terrain.terrainData.treeInstances
            );

            // Calcular el indice actual teniendo en cuenta
            // los arboles eliminados anteriormente
            int currentIndex = index;

            foreach (int removedIndex in _removedTreeIndices)
            {
                if (removedIndex < index)
                {
                    currentIndex--;
                }
            }

            // Comprobar el indice antes de eliminar
            if (currentIndex < 0 || currentIndex >= trees.Count)
            {
                Debug.LogWarning(
                    "No se pudo eliminar el arbol: " + index
                );
                return;
            }

            trees.RemoveAt(currentIndex);

            _terrain.terrainData.treeInstances = trees.ToArray();

            // Registrar el arbol eliminado
            _removedTreeIndices.Add(index);
        }

        #endregion
    }
}
