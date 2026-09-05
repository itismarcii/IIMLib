using System.Collections.Generic;
using CanvasSystem.Scriptable;
using UnityEditor;
using UnityEngine;

namespace CanvasSystem
{
    public class CanvasService : ICanvasService
    {
        private readonly Dictionary<string, ICanvasIdentifier> _CanvasIdentifiers = new();
        private readonly Dictionary<string, IUIIdentifier> _UIIdentifiers = new();
        
        public void Initialize()
        {
            _CanvasIdentifiers.Clear();
            _UIIdentifiers.Clear();

#if UNITY_EDITOR
            var uiByPrefabRoot = new Dictionary<GameObject, IUIIdentifier>();
            var canvasUsageByUiPrefab = new Dictionary<GameObject, HashSet<string>>();

            foreach (var canvas in CanvasIdentifierScriptable.Identifiers)
            {
                if (canvas == null || canvas.CanvasObject == null)
                {
                    Debug.LogWarning("CanvasIdentifier contains a null canvas prefab reference.");
                    continue;
                }

                _CanvasIdentifiers[canvas.Identifier] = canvas;
            }

            foreach (var ui in UIIdentifierScriptable.Identifiers)
            {
                if (ui == null || ui.UIGameObject == null)
                {
                    Debug.LogWarning("UIIdentifier contains a null UI prefab reference.");
                    continue;
                }

                _UIIdentifiers[ui.Identifier] = ui;
                uiByPrefabRoot[ui.UIGameObject] = ui;
                canvasUsageByUiPrefab[ui.UIGameObject] = new HashSet<string>();
            }

            foreach (var canvas in _CanvasIdentifiers.Values)
            {
                var transforms = canvas.CanvasObject.GetComponentsInChildren<Transform>(true);

                foreach (var transform in transforms)
                {
                    var nearestInstanceRoot = PrefabUtility.GetNearestPrefabInstanceRoot(transform.gameObject);
                    if (nearestInstanceRoot == null)
                        continue;

                    var sourcePrefabRoot = PrefabUtility.GetCorrespondingObjectFromSource(nearestInstanceRoot);
                    if (sourcePrefabRoot == null)
                        continue;

                    if (!uiByPrefabRoot.ContainsKey(sourcePrefabRoot))
                        continue;

                    canvasUsageByUiPrefab[sourcePrefabRoot].Add(canvas.Identifier);
                }
            }

            foreach (var pair in canvasUsageByUiPrefab)
            {
                if (pair.Value.Count <= 2)
                    continue;

                var ui = uiByPrefabRoot[pair.Key];
                Debug.LogWarning(
                    $"UI prefab '{ui.UIGameObject.name}' is used in more than two registered canvases ({pair.Value.Count}): {string.Join(", ", pair.Value)}",
                    pair.Key
                );
            }
#else
    foreach (var canvas in CanvasIdentifierScriptable.Identifiers)
    {
        if (canvas != null)
            _CanvasIdentifiers[canvas.Identifier] = canvas;
    }

    foreach (var ui in UIIdentifierScriptable.Identifiers)
    {
        if (ui != null)
            _UIIdentifiers[ui.Identifier] = ui;
    }
#endif

            foreach (var identifier in _CanvasIdentifiers.Values)
            {
                if (!identifier.Create) continue;
                
                var obj = Object.Instantiate(identifier.CanvasObject, identifier.CanvasObject.transform.position,
                    identifier.CanvasObject.transform.rotation);
                    
                obj.gameObject.SetActive(identifier.IsDefaultActive);
            }
        }

        public bool TryGetUI<T>(IUIIdentifier identifier, out T element)
        {
            element = default;
            
            if (identifier.Type != typeof(T) || !_UIIdentifiers.TryGetValue(identifier.Identifier, out var value)) return false;
            
            element = (T)value;
            return true;
        }

        public GameObject GetUI(IUIIdentifier identifier) => _UIIdentifiers.TryGetValue(identifier.Identifier, out var value) ? value.UIGameObject : null;

        public Canvas GetCanvas(ICanvasIdentifier identifier) => _CanvasIdentifiers.GetValueOrDefault(identifier.Identifier)?.CanvasObject;
    }
}
