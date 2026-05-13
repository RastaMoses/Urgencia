using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

public static class MyExtensions
{
#if UNITY_EDITOR
    // Gets value from SerializedProperty - even if value is nested
    public static object GetValue(this UnityEditor.SerializedProperty property)
    {
        object obj = property.serializedObject.targetObject;

        FieldInfo field = null;
        foreach (var path in property.propertyPath.Split('.'))
        {
            var type = obj.GetType();
            field = type.GetField(path);
            obj = field.GetValue(obj);
        }
        return obj;
    }

    // Sets value from SerializedProperty - even if value is nested
    public static void SetValue(this UnityEditor.SerializedProperty property, object val)
    {
        object obj = property.serializedObject.targetObject;

        List<KeyValuePair<FieldInfo, object>> list = new List<KeyValuePair<FieldInfo, object>>();

        FieldInfo field = null;
        foreach (var path in property.propertyPath.Split('.'))
        {
            var type = obj.GetType();
            field = type.GetField(path);
            list.Add(new KeyValuePair<FieldInfo, object>(field, obj));
            obj = field.GetValue(obj);
        }

        // Now set values of all objects, from child to parent
        for (int i = list.Count - 1; i >= 0; --i)
        {
            list[i].Key.SetValue(list[i].Value, val);
            // New 'val' object will be parent of current 'val' object
            val = list[i].Value;
        }
    }
#endif // UNITY_EDITOR
}

public abstract class DictionaryDrawer<TK, TV> : PropertyDrawer
{
    private SerializableDictionary<TK, TV> _Dictionary;
    private bool _Foldout;
    private const float kButtonWidth = 18f;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        CheckInitialize(property, label);
        if (_Foldout)
            return (_Dictionary.Count + 1) * 17f;
        return 17f;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        CheckInitialize(property, label);

        position.height = 17f;

        var foldoutRect = position;
        foldoutRect.width -= 2 * kButtonWidth;
        EditorGUI.BeginChangeCheck();
        _Foldout = EditorGUI.Foldout(foldoutRect, _Foldout, label, true);
        if (EditorGUI.EndChangeCheck())
            EditorPrefs.SetBool(label.text, _Foldout);

        var buttonRect = position;
        buttonRect.x = position.width - kButtonWidth + position.x;
        buttonRect.width = kButtonWidth + 2;

        if (GUI.Button(buttonRect, new GUIContent("+", "Add item"), EditorStyles.miniButton))
        {
            AddNewItem();
            EditorUtility.SetDirty(property.serializedObject.targetObject);
        }

        buttonRect.x -= kButtonWidth;

        if (GUI.Button(buttonRect, new GUIContent("X", "Clear dictionary"), EditorStyles.miniButtonRight))
        {
            ClearDictionary();
            EditorUtility.SetDirty(property.serializedObject.targetObject);
        }

        if (!_Foldout)
            return;

        foreach (var item in _Dictionary)
        {
            var key = item.Key;
            var value = item.Value;

            position.y += 17f;

            var keyRect = position;
            keyRect.width /= 2;
            keyRect.width -= 4;
            EditorGUI.BeginChangeCheck();
            var newKey = DoField(keyRect, typeof(TK), key);
            if (EditorGUI.EndChangeCheck())
            {
                try
                {
                    _Dictionary.Remove(key);
                    _Dictionary.Add(newKey, value);
                    EditorUtility.SetDirty(property.serializedObject.targetObject);
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }
                break;
            }

            var valueRect = position;
            valueRect.x = position.width / 2 + 15;
            valueRect.width = keyRect.width - kButtonWidth;
            EditorGUI.BeginChangeCheck();
            value = DoField(valueRect, typeof(TV), value);
            if (EditorGUI.EndChangeCheck())
            {
                _Dictionary[key] = value;
                EditorUtility.SetDirty(property.serializedObject.targetObject);
                break;
            }

            var removeRect = valueRect;
            removeRect.x = valueRect.xMax + 2;
            removeRect.width = kButtonWidth;
            if (GUI.Button(removeRect, new GUIContent("x", "Remove item"), EditorStyles.miniButtonRight))
            {
                RemoveItem(key);
                EditorUtility.SetDirty(property.serializedObject.targetObject);
                break;
            }

            property.serializedObject.Update();
            property.serializedObject.ApplyModifiedProperties();
        }
    }

    private void RemoveItem(TK key)
    {
        _Dictionary.Remove(key);
    }

    private void CheckInitialize(SerializedProperty property, GUIContent label)
    {
        if (_Dictionary == null)
        {
            var target = property.serializedObject.targetObject;
            _Dictionary = fieldInfo.GetValue(target) as SerializableDictionary<TK, TV>;
            if (_Dictionary == null)
            {
                _Dictionary = new SerializableDictionary<TK, TV>();
                fieldInfo.SetValue(target, _Dictionary);
            }

            _Foldout = EditorPrefs.GetBool(label.text);
        }
    }

    private static readonly Dictionary<Type, Func<Rect, object, object>> _Fields =
        new Dictionary<Type, Func<Rect, object, object>>()
        {
                { typeof(int), (rect, value) => EditorGUI.IntField(rect, (int)value) },
                { typeof(float), (rect, value) => EditorGUI.FloatField(rect, (float)value) },
                { typeof(string), (rect, value) => EditorGUI.TextField(rect, (string)value) },
                { typeof(bool), (rect, value) => EditorGUI.Toggle(rect, (bool)value) },
                { typeof(Vector2), (rect, value) => EditorGUI.Vector2Field(rect, GUIContent.none, (Vector2)value) },
                { typeof(Vector3), (rect, value) => EditorGUI.Vector3Field(rect, GUIContent.none, (Vector3)value) },
                { typeof(Bounds), (rect, value) => EditorGUI.BoundsField(rect, (Bounds)value) },
                { typeof(Rect), (rect, value) => EditorGUI.RectField(rect, (Rect)value) },
        };

    private static T DoField<T>(Rect rect, Type type, T value)
    {
        Func<Rect, object, object> field;
        if (_Fields.TryGetValue(type, out field))
            return (T)field(rect, value);

        if (type.IsEnum)
            return (T)(object)EditorGUI.EnumPopup(rect, (Enum)(object)value);

        if (typeof(UnityObject).IsAssignableFrom(type))
            return (T)(object)EditorGUI.ObjectField(rect, (UnityObject)(object)value, type, true);

        Debug.Log("Type is not supported: " + type);
        return value;
    }

    private void ClearDictionary()
    {
        _Dictionary.Clear();
    }

    private void AddNewItem()
    {
        TK key;
        if (typeof(TK) == typeof(string))
            key = (TK)(object)"";
        else key = default(TK);

        var value = default(TV);
        try
        {
            _Dictionary.Add(key, value);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }
}

public abstract class LootingPointsDictionaryDrawer<TK, TV> : PropertyDrawer
{
    private SerializableDictionary<TK, TV> _Dictionary;
    private bool _Foldout;
    private const float kButtonWidth = 18f;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        CheckInitialize(property.serializedObject.FindProperty("saveFile").FindPropertyRelative("LootingPointsData").FindPropertyRelative("allLootingPoints"), label);
        if (_Foldout)
            return (_Dictionary.Count + 1) * 17f;
        return 17f;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        CheckInitialize(property.serializedObject.FindProperty("saveFile").FindPropertyRelative("LootingPointsData").FindPropertyRelative("allLootingPoints"), label);

        position.height = 17f;

        var foldoutRect = position;
        foldoutRect.width -= 2 * kButtonWidth;
        EditorGUI.BeginChangeCheck();
        _Foldout = EditorGUI.Foldout(foldoutRect, _Foldout, label, true);
        if (EditorGUI.EndChangeCheck())
            EditorPrefs.SetBool(label.text, _Foldout);

        var buttonRect = position;
        buttonRect.x = position.width - kButtonWidth + position.x;
        buttonRect.width = kButtonWidth + 2;

        if (GUI.Button(buttonRect, new GUIContent("+", "Add item"), EditorStyles.miniButton))
        {
            AddNewItem();
        }

        buttonRect.x -= kButtonWidth;

        if (GUI.Button(buttonRect, new GUIContent("X", "Clear dictionary"), EditorStyles.miniButtonRight))
        {
            ClearDictionary();
        }

        if (!_Foldout)
            return;

        foreach (var item in _Dictionary)
        {
            var key = item.Key;
            var value = item.Value;

            position.y += 17f;

            var keyRect = position;
            keyRect.width /= 2;
            keyRect.width -= 4;
            EditorGUI.BeginChangeCheck();
            var newKey = DoField(keyRect, typeof(TK), key);
            if (EditorGUI.EndChangeCheck())
            {
                try
                {
                    _Dictionary.Remove(key);
                    _Dictionary.Add(newKey, value);
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }
                break;
            }

            var valueRect = position;
            valueRect.x = position.width / 2 + 15;
            valueRect.width = keyRect.width - kButtonWidth;
            EditorGUI.BeginChangeCheck();
            value = DoField(valueRect, typeof(TV), value);
            if (EditorGUI.EndChangeCheck())
            {
                _Dictionary[key] = value;
                break;
            }

            var removeRect = valueRect;
            removeRect.x = valueRect.xMax + 2;
            removeRect.width = kButtonWidth;
            if (GUI.Button(removeRect, new GUIContent("x", "Remove item"), EditorStyles.miniButtonRight))
            {
                RemoveItem(key);
                break;
            }
        }
    }

    private void RemoveItem(TK key)
    {
        _Dictionary.Remove(key);
    }

    private void CheckInitialize(SerializedProperty property, GUIContent label)
    {
        if (_Dictionary == null)
        {
            _Dictionary = property.GetValue() as SerializableDictionary<TK, TV>;
            Debug.Log(_Dictionary);

            _Foldout = EditorPrefs.GetBool(label.text);
        }
    }

    private static readonly Dictionary<Type, Func<Rect, object, object>> _Fields =
        new Dictionary<Type, Func<Rect, object, object>>()
        {
                { typeof(int), (rect, value) => EditorGUI.IntField(rect, (int)value) },
                { typeof(float), (rect, value) => EditorGUI.FloatField(rect, (float)value) },
                { typeof(string), (rect, value) => EditorGUI.TextField(rect, (string)value) },
                { typeof(bool), (rect, value) => EditorGUI.Toggle(rect, (bool)value) },
                { typeof(Vector2), (rect, value) => EditorGUI.Vector2Field(rect, GUIContent.none, (Vector2)value) },
                { typeof(Vector3), (rect, value) => EditorGUI.Vector3Field(rect, GUIContent.none, (Vector3)value) },
                { typeof(Bounds), (rect, value) => EditorGUI.BoundsField(rect, (Bounds)value) },
                { typeof(Rect), (rect, value) => EditorGUI.RectField(rect, (Rect)value) },
        };

    private static T DoField<T>(Rect rect, Type type, T value)
    {
        Func<Rect, object, object> field;
        if (_Fields.TryGetValue(type, out field))
            return (T)field(rect, value);

        if (type.IsEnum)
            return (T)(object)EditorGUI.EnumPopup(rect, (Enum)(object)value);

        if (typeof(UnityObject).IsAssignableFrom(type))
            return (T)(object)EditorGUI.ObjectField(rect, (UnityObject)(object)value, type, true);

        return value;
    }

    private void ClearDictionary()
    {
        _Dictionary.Clear();
    }

    private void AddNewItem()
    {
        TK key;
        if (typeof(TK) == typeof(string))
            key = (TK)(object)"";
        else key = default(TK);

        var value = default(TV);
        try
        {
            _Dictionary.Add(key, value);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }
}

public abstract class ItemsInWorldDictionaryDrawer<TK, TV> : PropertyDrawer
{
    private SerializableDictionary<TK, TV> _Dictionary;
    private bool _Foldout;
    private const float kButtonWidth = 18f;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        CheckInitialize(property.serializedObject.FindProperty("saveFile").FindPropertyRelative("ItemsInWorldData").FindPropertyRelative("allItemsInWorld"), label);
        if (_Foldout)
            return (_Dictionary.Count + 1) * 17f;
        return 17f;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        CheckInitialize(property.serializedObject.FindProperty("saveFile").FindPropertyRelative("ItemsInWorldData").FindPropertyRelative("allItemsInWorld"), label);

        position.height = 17f;

        var foldoutRect = position;
        foldoutRect.width -= 2 * kButtonWidth;
        EditorGUI.BeginChangeCheck();
        _Foldout = EditorGUI.Foldout(foldoutRect, _Foldout, label, true);
        if (EditorGUI.EndChangeCheck())
            EditorPrefs.SetBool(label.text, _Foldout);

        var buttonRect = position;
        buttonRect.x = position.width - kButtonWidth + position.x;
        buttonRect.width = kButtonWidth + 2;

        if (GUI.Button(buttonRect, new GUIContent("+", "Add item"), EditorStyles.miniButton))
        {
            AddNewItem();
        }

        buttonRect.x -= kButtonWidth;

        if (GUI.Button(buttonRect, new GUIContent("X", "Clear dictionary"), EditorStyles.miniButtonRight))
        {
            ClearDictionary();
        }

        if (!_Foldout)
            return;

        foreach (var item in _Dictionary)
        {
            var key = item.Key;
            var value = item.Value;

            position.y += 17f;

            var keyRect = position;
            keyRect.width /= 2;
            keyRect.width -= 4;
            EditorGUI.BeginChangeCheck();
            var newKey = DoField(keyRect, typeof(TK), key);
            if (EditorGUI.EndChangeCheck())
            {
                try
                {
                    _Dictionary.Remove(key);
                    _Dictionary.Add(newKey, value);
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }
                break;
            }

            var valueRect = position;
            valueRect.x = position.width / 2 + 15;
            valueRect.width = keyRect.width - kButtonWidth;
            EditorGUI.BeginChangeCheck();
            value = DoField(valueRect, typeof(TV), value);
            if (EditorGUI.EndChangeCheck())
            {
                _Dictionary[key] = value;
                break;
            }

            var removeRect = valueRect;
            removeRect.x = valueRect.xMax + 2;
            removeRect.width = kButtonWidth;
            if (GUI.Button(removeRect, new GUIContent("x", "Remove item"), EditorStyles.miniButtonRight))
            {
                RemoveItem(key);
                break;
            }
        }
    }

    private void RemoveItem(TK key)
    {
        _Dictionary.Remove(key);
    }

    private void CheckInitialize(SerializedProperty property, GUIContent label)
    {
        if (_Dictionary == null)
        {
            _Dictionary = property.GetValue() as SerializableDictionary<TK, TV>;
            Debug.Log(_Dictionary);

            _Foldout = EditorPrefs.GetBool(label.text);
        }
    }

    private static readonly Dictionary<Type, Func<Rect, object, object>> _Fields =
        new Dictionary<Type, Func<Rect, object, object>>()
        {
                { typeof(int), (rect, value) => EditorGUI.IntField(rect, (int)value) },
                { typeof(float), (rect, value) => EditorGUI.FloatField(rect, (float)value) },
                { typeof(string), (rect, value) => EditorGUI.TextField(rect, (string)value) },
                { typeof(bool), (rect, value) => EditorGUI.Toggle(rect, (bool)value) },
                { typeof(Vector2), (rect, value) => EditorGUI.Vector2Field(rect, GUIContent.none, (Vector2)value) },
                { typeof(Vector3), (rect, value) => EditorGUI.Vector3Field(rect, GUIContent.none, (Vector3)value) },
                { typeof(Bounds), (rect, value) => EditorGUI.BoundsField(rect, (Bounds)value) },
                { typeof(Rect), (rect, value) => EditorGUI.RectField(rect, (Rect)value) },
        };

    private static T DoField<T>(Rect rect, Type type, T value)
    {
        Func<Rect, object, object> field;
        if (_Fields.TryGetValue(type, out field))
            return (T)field(rect, value);

        if (type.IsEnum)
            return (T)(object)EditorGUI.EnumPopup(rect, (Enum)(object)value);

        if (typeof(UnityObject).IsAssignableFrom(type))
            return (T)(object)EditorGUI.ObjectField(rect, (UnityObject)(object)value, type, true);

        return value;
    }

    private void ClearDictionary()
    {
        _Dictionary.Clear();
    }

    private void AddNewItem()
    {
        TK key;
        if (typeof(TK) == typeof(string))
            key = (TK)(object)"";
        else key = default(TK);

        var value = default(TV);
        try
        {
            _Dictionary.Add(key, value);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }
}

public abstract class CreatedItemsInWorldDictionaryDrawer<TK, TV> : PropertyDrawer
{
    private SerializableDictionary<TK, TV> _Dictionary;
    private bool _Foldout;
    private const float kButtonWidth = 18f;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        CheckInitialize(property.serializedObject.FindProperty("saveFile").FindPropertyRelative("CreatedItemsInWorldData").FindPropertyRelative("allCreatedItemsInWorld"), label);
        if (_Foldout)
            return (_Dictionary.Count + 1) * 17f;
        return 17f;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        CheckInitialize(property.serializedObject.FindProperty("saveFile").FindPropertyRelative("CreatedItemsInWorldData").FindPropertyRelative("allCreatedItemsInWorld"), label);

        position.height = 17f;

        var foldoutRect = position;
        foldoutRect.width -= 2 * kButtonWidth;
        EditorGUI.BeginChangeCheck();
        _Foldout = EditorGUI.Foldout(foldoutRect, _Foldout, label, true);
        if (EditorGUI.EndChangeCheck())
            EditorPrefs.SetBool(label.text, _Foldout);

        var buttonRect = position;
        buttonRect.x = position.width - kButtonWidth + position.x;
        buttonRect.width = kButtonWidth + 2;

        if (GUI.Button(buttonRect, new GUIContent("+", "Add item"), EditorStyles.miniButton))
        {
            AddNewItem();
        }

        buttonRect.x -= kButtonWidth;

        if (GUI.Button(buttonRect, new GUIContent("X", "Clear dictionary"), EditorStyles.miniButtonRight))
        {
            ClearDictionary();
        }

        if (!_Foldout)
            return;

        foreach (var item in _Dictionary)
        {
            var key = item.Key;
            var value = item.Value;

            position.y += 17f;

            var keyRect = position;
            keyRect.width /= 2;
            keyRect.width -= 4;
            EditorGUI.BeginChangeCheck();
            var newKey = DoField(keyRect, typeof(TK), key);
            if (EditorGUI.EndChangeCheck())
            {
                try
                {
                    _Dictionary.Remove(key);
                    _Dictionary.Add(newKey, value);
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }
                break;
            }

            var valueRect = position;
            valueRect.x = position.width / 2 + 15;
            valueRect.width = keyRect.width - kButtonWidth;
            EditorGUI.BeginChangeCheck();
            value = DoField(valueRect, typeof(TV), value);
            if (EditorGUI.EndChangeCheck())
            {
                _Dictionary[key] = value;
                break;
            }

            var removeRect = valueRect;
            removeRect.x = valueRect.xMax + 2;
            removeRect.width = kButtonWidth;
            if (GUI.Button(removeRect, new GUIContent("x", "Remove item"), EditorStyles.miniButtonRight))
            {
                RemoveItem(key);
                break;
            }
        }
    }

    private void RemoveItem(TK key)
    {
        _Dictionary.Remove(key);
    }

    private void CheckInitialize(SerializedProperty property, GUIContent label)
    {
        if (_Dictionary == null)
        {
            _Dictionary = property.GetValue() as SerializableDictionary<TK, TV>;
            Debug.Log(_Dictionary);

            _Foldout = EditorPrefs.GetBool(label.text);
        }
    }

    private static readonly Dictionary<Type, Func<Rect, object, object>> _Fields =
        new Dictionary<Type, Func<Rect, object, object>>()
        {
                { typeof(int), (rect, value) => EditorGUI.IntField(rect, (int)value) },
                { typeof(float), (rect, value) => EditorGUI.FloatField(rect, (float)value) },
                { typeof(string), (rect, value) => EditorGUI.TextField(rect, (string)value) },
                { typeof(bool), (rect, value) => EditorGUI.Toggle(rect, (bool)value) },
                { typeof(Vector2), (rect, value) => EditorGUI.Vector2Field(rect, GUIContent.none, (Vector2)value) },
                { typeof(Vector3), (rect, value) => EditorGUI.Vector3Field(rect, GUIContent.none, (Vector3)value) },
                { typeof(Bounds), (rect, value) => EditorGUI.BoundsField(rect, (Bounds)value) },
                { typeof(Rect), (rect, value) => EditorGUI.RectField(rect, (Rect)value) },
        };

    private static T DoField<T>(Rect rect, Type type, T value)
    {
        Func<Rect, object, object> field;
        if (_Fields.TryGetValue(type, out field))
            return (T)field(rect, value);

        if (type.IsEnum)
            return (T)(object)EditorGUI.EnumPopup(rect, (Enum)(object)value);

        if (typeof(UnityObject).IsAssignableFrom(type))
            return (T)(object)EditorGUI.ObjectField(rect, (UnityObject)(object)value, type, true);

        return value;
    }

    private void ClearDictionary()
    {
        _Dictionary.Clear();
    }

    private void AddNewItem()
    {
        TK key;
        if (typeof(TK) == typeof(string))
            key = (TK)(object)"";
        else key = default(TK);

        var value = default(TV);
        try
        {
            _Dictionary.Add(key, value);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }
}

public abstract class DoorDataDictionaryDrawer<TK, TV> : PropertyDrawer
{
    private SerializableDictionary<TK, TV> _Dictionary;
    private bool _Foldout;
    private const float kButtonWidth = 18f;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        CheckInitialize(property.serializedObject.FindProperty("saveFile").FindPropertyRelative("DoorData").FindPropertyRelative("allDoors"), label);
        if (_Foldout)
            return (_Dictionary.Count + 1) * 17f;
        return 17f;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        CheckInitialize(property.serializedObject.FindProperty("saveFile").FindPropertyRelative("DoorData").FindPropertyRelative("allDoors"), label);

        position.height = 17f;

        var foldoutRect = position;
        foldoutRect.width -= 2 * kButtonWidth;
        EditorGUI.BeginChangeCheck();
        _Foldout = EditorGUI.Foldout(foldoutRect, _Foldout, label, true);
        if (EditorGUI.EndChangeCheck())
            EditorPrefs.SetBool(label.text, _Foldout);

        var buttonRect = position;
        buttonRect.x = position.width - kButtonWidth + position.x;
        buttonRect.width = kButtonWidth + 2;

        if (GUI.Button(buttonRect, new GUIContent("+", "Add item"), EditorStyles.miniButton))
        {
            AddNewItem();
        }

        buttonRect.x -= kButtonWidth;

        if (GUI.Button(buttonRect, new GUIContent("X", "Clear dictionary"), EditorStyles.miniButtonRight))
        {
            ClearDictionary();
        }

        if (!_Foldout)
            return;

        foreach (var item in _Dictionary)
        {
            var key = item.Key;
            var value = item.Value;

            position.y += 17f;

            var keyRect = position;
            keyRect.width /= 2;
            keyRect.width -= 4;
            EditorGUI.BeginChangeCheck();
            var newKey = DoField(keyRect, typeof(TK), key);
            if (EditorGUI.EndChangeCheck())
            {
                try
                {
                    _Dictionary.Remove(key);
                    _Dictionary.Add(newKey, value);
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }
                break;
            }

            var valueRect = position;
            valueRect.x = position.width / 2 + 15;
            valueRect.width = keyRect.width - kButtonWidth;
            EditorGUI.BeginChangeCheck();
            value = DoField(valueRect, typeof(TV), value);
            if (EditorGUI.EndChangeCheck())
            {
                _Dictionary[key] = value;
                break;
            }

            var removeRect = valueRect;
            removeRect.x = valueRect.xMax + 2;
            removeRect.width = kButtonWidth;
            if (GUI.Button(removeRect, new GUIContent("x", "Remove item"), EditorStyles.miniButtonRight))
            {
                RemoveItem(key);
                break;
            }
        }
    }

    private void RemoveItem(TK key)
    {
        _Dictionary.Remove(key);
    }

    private void CheckInitialize(SerializedProperty property, GUIContent label)
    {
        if (_Dictionary == null)
        {
            _Dictionary = property.GetValue() as SerializableDictionary<TK, TV>;
            Debug.Log(_Dictionary);

            _Foldout = EditorPrefs.GetBool(label.text);
        }
    }

    private static readonly Dictionary<Type, Func<Rect, object, object>> _Fields =
        new Dictionary<Type, Func<Rect, object, object>>()
        {
                { typeof(int), (rect, value) => EditorGUI.IntField(rect, (int)value) },
                { typeof(float), (rect, value) => EditorGUI.FloatField(rect, (float)value) },
                { typeof(string), (rect, value) => EditorGUI.TextField(rect, (string)value) },
                { typeof(bool), (rect, value) => EditorGUI.Toggle(rect, (bool)value) },
                { typeof(Vector2), (rect, value) => EditorGUI.Vector2Field(rect, GUIContent.none, (Vector2)value) },
                { typeof(Vector3), (rect, value) => EditorGUI.Vector3Field(rect, GUIContent.none, (Vector3)value) },
                { typeof(Bounds), (rect, value) => EditorGUI.BoundsField(rect, (Bounds)value) },
                { typeof(Rect), (rect, value) => EditorGUI.RectField(rect, (Rect)value) },
        };

    private static T DoField<T>(Rect rect, Type type, T value)
    {
        Func<Rect, object, object> field;
        if (_Fields.TryGetValue(type, out field))
            return (T)field(rect, value);

        if (type.IsEnum)
            return (T)(object)EditorGUI.EnumPopup(rect, (Enum)(object)value);

        if (typeof(UnityObject).IsAssignableFrom(type))
            return (T)(object)EditorGUI.ObjectField(rect, (UnityObject)(object)value, type, true);

        return value;
    }

    private void ClearDictionary()
    {
        _Dictionary.Clear();
    }

    private void AddNewItem()
    {
        TK key;
        if (typeof(TK) == typeof(string))
            key = (TK)(object)"";
        else key = default(TK);

        var value = default(TV);
        try
        {
            _Dictionary.Add(key, value);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }
}

public abstract class AIDictionaryDrawer<TK, TV> : PropertyDrawer
{
    private SerializableDictionary<TK, TV> _Dictionary;
    private bool _Foldout;
    private const float kButtonWidth = 18f;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        CheckInitialize(property.serializedObject.FindProperty("saveFile").FindPropertyRelative("AIData").FindPropertyRelative("aiDictionary"), label);
        if (_Foldout)
            return (_Dictionary.Count + 1) * 17f;
        return 17f;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        CheckInitialize(property.serializedObject.FindProperty("saveFile").FindPropertyRelative("AIData").FindPropertyRelative("aiDictionary"), label);

        position.height = 17f;

        var foldoutRect = position;
        foldoutRect.width -= 2 * kButtonWidth;
        EditorGUI.BeginChangeCheck();
        _Foldout = EditorGUI.Foldout(foldoutRect, _Foldout, label, true);
        if (EditorGUI.EndChangeCheck())
            EditorPrefs.SetBool(label.text, _Foldout);

        var buttonRect = position;
        buttonRect.x = position.width - kButtonWidth + position.x;
        buttonRect.width = kButtonWidth + 2;

        if (GUI.Button(buttonRect, new GUIContent("+", "Add item"), EditorStyles.miniButton))
        {
            AddNewItem();
        }

        buttonRect.x -= kButtonWidth;

        if (GUI.Button(buttonRect, new GUIContent("X", "Clear dictionary"), EditorStyles.miniButtonRight))
        {
            ClearDictionary();
        }

        if (!_Foldout)
            return;

        foreach (var item in _Dictionary)
        {
            var key = item.Key;
            var value = item.Value;

            position.y += 17f;

            var keyRect = position;
            keyRect.width /= 2;
            keyRect.width -= 4;
            EditorGUI.BeginChangeCheck();
            var newKey = DoField(keyRect, typeof(TK), key);
            if (EditorGUI.EndChangeCheck())
            {
                try
                {
                    _Dictionary.Remove(key);
                    _Dictionary.Add(newKey, value);
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }
                break;
            }

            var valueRect = position;
            valueRect.x = position.width / 2 + 15;
            valueRect.width = keyRect.width - kButtonWidth;
            EditorGUI.BeginChangeCheck();
            value = DoField(valueRect, typeof(TV), value);
            if (EditorGUI.EndChangeCheck())
            {
                _Dictionary[key] = value;
                break;
            }

            var removeRect = valueRect;
            removeRect.x = valueRect.xMax + 2;
            removeRect.width = kButtonWidth;
            if (GUI.Button(removeRect, new GUIContent("x", "Remove item"), EditorStyles.miniButtonRight))
            {
                RemoveItem(key);
                break;
            }
        }
    }

    private void RemoveItem(TK key)
    {
        _Dictionary.Remove(key);
    }

    private void CheckInitialize(SerializedProperty property, GUIContent label)
    {
        if (_Dictionary == null)
        {
            _Dictionary = property.GetValue() as SerializableDictionary<TK, TV>;
            Debug.Log(_Dictionary);

            _Foldout = EditorPrefs.GetBool(label.text);
        }
    }

    private static readonly Dictionary<Type, Func<Rect, object, object>> _Fields =
        new Dictionary<Type, Func<Rect, object, object>>()
        {
                { typeof(int), (rect, value) => EditorGUI.IntField(rect, (int)value) },
                { typeof(float), (rect, value) => EditorGUI.FloatField(rect, (float)value) },
                { typeof(string), (rect, value) => EditorGUI.TextField(rect, (string)value) },
                { typeof(bool), (rect, value) => EditorGUI.Toggle(rect, (bool)value) },
                { typeof(Vector2), (rect, value) => EditorGUI.Vector2Field(rect, GUIContent.none, (Vector2)value) },
                { typeof(Vector3), (rect, value) => EditorGUI.Vector3Field(rect, GUIContent.none, (Vector3)value) },
                { typeof(Bounds), (rect, value) => EditorGUI.BoundsField(rect, (Bounds)value) },
                { typeof(Rect), (rect, value) => EditorGUI.RectField(rect, (Rect)value) },
        };

    private static T DoField<T>(Rect rect, Type type, T value)
    {
        Func<Rect, object, object> field;
        if (_Fields.TryGetValue(type, out field))
            return (T)field(rect, value);

        if (type.IsEnum)
            return (T)(object)EditorGUI.EnumPopup(rect, (Enum)(object)value);

        if (typeof(UnityObject).IsAssignableFrom(type))
            return (T)(object)EditorGUI.ObjectField(rect, (UnityObject)(object)value, type, true);

        return value;
    }

    private void ClearDictionary()
    {
        _Dictionary.Clear();
    }

    private void AddNewItem()
    {
        TK key;
        if (typeof(TK) == typeof(string))
            key = (TK)(object)"";
        else key = default(TK);

        var value = default(TV);
        try
        {
            _Dictionary.Add(key, value);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }
}

public abstract class AICellDictionaryDrawer<TK, TV> : PropertyDrawer
{
    private SerializableDictionary<TK, TV> _Dictionary;
    private bool _Foldout;
    private const float kButtonWidth = 18f;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        CheckInitialize(property.serializedObject.FindProperty("saveFile").FindPropertyRelative("AIData").FindPropertyRelative("aiCellDictionary"), label);
        if (_Foldout)
            return (_Dictionary.Count + 1) * 17f;
        return 17f;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        CheckInitialize(property.serializedObject.FindProperty("saveFile").FindPropertyRelative("AIData").FindPropertyRelative("aiCellDictionary"), label);

        position.height = 17f;

        var foldoutRect = position;
        foldoutRect.width -= 2 * kButtonWidth;
        EditorGUI.BeginChangeCheck();
        _Foldout = EditorGUI.Foldout(foldoutRect, _Foldout, label, true);
        if (EditorGUI.EndChangeCheck())
            EditorPrefs.SetBool(label.text, _Foldout);

        var buttonRect = position;
        buttonRect.x = position.width - kButtonWidth + position.x;
        buttonRect.width = kButtonWidth + 2;

        if (GUI.Button(buttonRect, new GUIContent("+", "Add item"), EditorStyles.miniButton))
        {
            AddNewItem();
        }

        buttonRect.x -= kButtonWidth;

        if (GUI.Button(buttonRect, new GUIContent("X", "Clear dictionary"), EditorStyles.miniButtonRight))
        {
            ClearDictionary();
        }

        if (!_Foldout)
            return;

        foreach (var item in _Dictionary)
        {
            var key = item.Key;
            var value = item.Value;

            position.y += 17f;

            var keyRect = position;
            keyRect.width /= 2;
            keyRect.width -= 4;
            EditorGUI.BeginChangeCheck();
            var newKey = DoField(keyRect, typeof(TK), key);
            if (EditorGUI.EndChangeCheck())
            {
                try
                {
                    _Dictionary.Remove(key);
                    _Dictionary.Add(newKey, value);
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }
                break;
            }

            var valueRect = position;
            valueRect.x = position.width / 2 + 15;
            valueRect.width = keyRect.width - kButtonWidth;
            EditorGUI.BeginChangeCheck();
            value = DoField(valueRect, typeof(TV), value);
            if (EditorGUI.EndChangeCheck())
            {
                _Dictionary[key] = value;
                break;
            }

            var removeRect = valueRect;
            removeRect.x = valueRect.xMax + 2;
            removeRect.width = kButtonWidth;
            if (GUI.Button(removeRect, new GUIContent("x", "Remove item"), EditorStyles.miniButtonRight))
            {
                RemoveItem(key);
                break;
            }
        }
    }

    private void RemoveItem(TK key)
    {
        _Dictionary.Remove(key);
    }

    private void CheckInitialize(SerializedProperty property, GUIContent label)
    {
        if (_Dictionary == null)
        {
            _Dictionary = property.GetValue() as SerializableDictionary<TK, TV>;
            Debug.Log(_Dictionary);

            _Foldout = EditorPrefs.GetBool(label.text);
        }
    }

    private static readonly Dictionary<Type, Func<Rect, object, object>> _Fields =
        new Dictionary<Type, Func<Rect, object, object>>()
        {
                { typeof(int), (rect, value) => EditorGUI.IntField(rect, (int)value) },
                { typeof(float), (rect, value) => EditorGUI.FloatField(rect, (float)value) },
                { typeof(string), (rect, value) => EditorGUI.TextField(rect, (string)value) },
                { typeof(bool), (rect, value) => EditorGUI.Toggle(rect, (bool)value) },
                { typeof(Vector2), (rect, value) => EditorGUI.Vector2Field(rect, GUIContent.none, (Vector2)value) },
                { typeof(Vector3), (rect, value) => EditorGUI.Vector3Field(rect, GUIContent.none, (Vector3)value) },
                { typeof(Bounds), (rect, value) => EditorGUI.BoundsField(rect, (Bounds)value) },
                { typeof(Rect), (rect, value) => EditorGUI.RectField(rect, (Rect)value) },
        };

    private static T DoField<T>(Rect rect, Type type, T value)
    {
        Func<Rect, object, object> field;
        if (_Fields.TryGetValue(type, out field))
            return (T)field(rect, value);

        if (type.IsEnum)
            return (T)(object)EditorGUI.EnumPopup(rect, (Enum)(object)value);

        if (typeof(UnityObject).IsAssignableFrom(type))
            return (T)(object)EditorGUI.ObjectField(rect, (UnityObject)(object)value, type, true);

        return value;
    }

    private void ClearDictionary()
    {
        _Dictionary.Clear();
    }

    private void AddNewItem()
    {
        TK key;
        if (typeof(TK) == typeof(string))
            key = (TK)(object)"";
        else key = default(TK);

        var value = default(TV);
        try
        {
            _Dictionary.Add(key, value);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }
}


public abstract class QuestIDQuestDataDictionaryDrawer<TK, TV> : PropertyDrawer
{
    private SerializableDictionary<TK, TV> _Dictionary;
    private bool _Foldout;
    private const float kButtonWidth = 18f;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        CheckInitialize(property.serializedObject.FindProperty("saveFile").FindPropertyRelative("QuestsData").FindPropertyRelative("allQuests"), label);
        if (_Foldout)
            return (_Dictionary.Count + 1) * 17f;
        return 17f;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        CheckInitialize(property.serializedObject.FindProperty("saveFile").FindPropertyRelative("QuestsData").FindPropertyRelative("allQuests"), label);

        position.height = 17f;

        var foldoutRect = position;
        foldoutRect.width -= 2 * kButtonWidth;
        EditorGUI.BeginChangeCheck();
        _Foldout = EditorGUI.Foldout(foldoutRect, _Foldout, label, true);
        if (EditorGUI.EndChangeCheck())
            EditorPrefs.SetBool(label.text, _Foldout);

        var buttonRect = position;
        buttonRect.x = position.width - kButtonWidth + position.x;
        buttonRect.width = kButtonWidth + 2;

        if (GUI.Button(buttonRect, new GUIContent("+", "Add item"), EditorStyles.miniButton))
        {
            AddNewItem();
        }

        buttonRect.x -= kButtonWidth;

        if (GUI.Button(buttonRect, new GUIContent("X", "Clear dictionary"), EditorStyles.miniButtonRight))
        {
            ClearDictionary();
        }

        if (!_Foldout)
            return;

        foreach (var item in _Dictionary)
        {
            var key = item.Key;
            var value = item.Value;

            position.y += 17f;

            var keyRect = position;
            keyRect.width /= 2;
            keyRect.width -= 4;
            EditorGUI.BeginChangeCheck();
            var newKey = DoField(keyRect, typeof(TK), key);
            if (EditorGUI.EndChangeCheck())
            {
                try
                {
                    _Dictionary.Remove(key);
                    _Dictionary.Add(newKey, value);
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }
                break;
            }

            var valueRect = position;
            valueRect.x = position.width / 2 + 15;
            valueRect.width = keyRect.width - kButtonWidth;
            EditorGUI.BeginChangeCheck();
            value = DoField(valueRect, typeof(TV), value);
            if (EditorGUI.EndChangeCheck())
            {
                _Dictionary[key] = value;
                break;
            }

            var removeRect = valueRect;
            removeRect.x = valueRect.xMax + 2;
            removeRect.width = kButtonWidth;
            if (GUI.Button(removeRect, new GUIContent("x", "Remove item"), EditorStyles.miniButtonRight))
            {
                RemoveItem(key);
                break;
            }
        }
    }

    private void RemoveItem(TK key)
    {
        _Dictionary.Remove(key);
    }

    private void CheckInitialize(SerializedProperty property, GUIContent label)
    {
        if (_Dictionary == null)
        {
            _Dictionary = property.GetValue() as SerializableDictionary<TK, TV>;
            Debug.Log(_Dictionary);

            _Foldout = EditorPrefs.GetBool(label.text);
        }
    }

    private static readonly Dictionary<Type, Func<Rect, object, object>> _Fields =
        new Dictionary<Type, Func<Rect, object, object>>()
        {
                { typeof(int), (rect, value) => EditorGUI.IntField(rect, (int)value) },
                { typeof(float), (rect, value) => EditorGUI.FloatField(rect, (float)value) },
                { typeof(string), (rect, value) => EditorGUI.TextField(rect, (string)value) },
                { typeof(bool), (rect, value) => EditorGUI.Toggle(rect, (bool)value) },
                { typeof(Vector2), (rect, value) => EditorGUI.Vector2Field(rect, GUIContent.none, (Vector2)value) },
                { typeof(Vector3), (rect, value) => EditorGUI.Vector3Field(rect, GUIContent.none, (Vector3)value) },
                { typeof(Bounds), (rect, value) => EditorGUI.BoundsField(rect, (Bounds)value) },
                { typeof(Rect), (rect, value) => EditorGUI.RectField(rect, (Rect)value) },
        };

    private static T DoField<T>(Rect rect, Type type, T value)
    {
        Func<Rect, object, object> field;
        if (_Fields.TryGetValue(type, out field))
            return (T)field(rect, value);

        if (type.IsEnum)
            return (T)(object)EditorGUI.EnumPopup(rect, (Enum)(object)value);

        if (typeof(UnityObject).IsAssignableFrom(type))
            return (T)(object)EditorGUI.ObjectField(rect, (UnityObject)(object)value, type, true);

        return value;
    }

    private void ClearDictionary()
    {
        _Dictionary.Clear();
    }

    private void AddNewItem()
    {
        TK key;
        if (typeof(TK) == typeof(string))
            key = (TK)(object)"";
        else key = default(TK);

        var value = default(TV);
        try
        {
            _Dictionary.Add(key, value);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }
}



[CustomPropertyDrawer(typeof(RPGCreationKit.PersistentReferences.PersistentReferencesDictionary))]
public class MyDictionaryDrawer1 : DictionaryDrawer<string, RPGCreationKit.PersistentReferences.PersistentReference> { }

[CustomPropertyDrawer(typeof(RPGCreationKit.ItemsDatabaseDictionary))]
public class MyDictionaryDrawer2 : DictionaryDrawer<string, RPGCreationKit.Item> { }

[CustomPropertyDrawer(typeof(RPGCreationKit.IconsDatabaseDictionary))]
public class MyDictionaryDrawer3 : DictionaryDrawer<string, Sprite> { }

[CustomPropertyDrawer(typeof(RPGCreationKit.FactionsDatabaseDictionary))]
public class MyDictionaryDrawer4 : DictionaryDrawer<string, RPGCreationKit.Faction> { }

[CustomPropertyDrawer(typeof(RPGCreationKit.WorldspacesDatabaseDictionary))]
public class MyDictionaryDrawer5 : DictionaryDrawer<string, RPGCreationKit.CellsSystem.Worldspace> { }

[CustomPropertyDrawer(typeof(RPGCreationKit.QuestsDatabaseDictionary))]
public class MyDictionaryDrawer6 : DictionaryDrawer<string, RPGCreationKit.Quest> { }


[CustomPropertyDrawer(typeof(RPGCreationKit.SaveSystem.LootingPointsDictionary))]
public class MyDictionaryDrawer7 : LootingPointsDictionaryDrawer<string, RPGCreationKit.SaveSystem.LootingPointSaveData> { }


[CustomPropertyDrawer(typeof(RPGCreationKit.SaveSystem.ItemsInWorldDictionary))]
public class MyDictionaryDrawer8 : ItemsInWorldDictionaryDrawer<string, RPGCreationKit.SaveSystem.ItemInWorldSaveData> { }

[CustomPropertyDrawer(typeof(RPGCreationKit.SaveSystem.CreatedItemsInWorldDictionary))]
public class MyDictionaryDrawer9 : CreatedItemsInWorldDictionaryDrawer<string, RPGCreationKit.SaveSystem.CreatedItemInWorldCollection> { }


[CustomPropertyDrawer(typeof(RPGCreationKit.SaveSystem.DoorsDataDictionary))]
public class MyDictionaryDrawer10: DoorDataDictionaryDrawer<string, RPGCreationKit.SaveSystem.DoorSaveData> { }


[CustomPropertyDrawer(typeof(RPGCreationKit.AIDatabaseFileDictionary))]
public class MyDictionaryDrawer11 : DictionaryDrawer<string, GameObject> { }


[CustomPropertyDrawer(typeof(RPGCreationKit.CellsSystem.AIInWorldDictionary))]
public class MyDictionaryDrawer12 : DictionaryDrawer<string, RPGCreationKit.AI.RckAI> { }


[CustomPropertyDrawer(typeof(RPGCreationKit.DialoguesDatabaseDictionary))]
public class MyDictionaryDrawer13 : DictionaryDrawer<string, RPGCreationKit.DialogueSystem.DialogueGraph> { }

[CustomPropertyDrawer(typeof(RPGCreationKit.BehavioursDatabaseDictionary))]
public class MyDictionaryDrawer14 : DictionaryDrawer<string, RPGCreationKit.BehaviourTree.RPGCK_BT> { }


[CustomPropertyDrawer(typeof(RPGCreationKit.SaveSystem.AIDictionary))]
public class MyDictionaryDrawer15 : AIDictionaryDrawer<string, RPGCreationKit.SaveSystem.AISaveData> { }

[CustomPropertyDrawer(typeof(RPGCreationKit.SaveSystem.AICellDictionary))]
public class MyDictionaryDrawer16 : AICellDictionaryDrawer<string, RPGCreationKit.SaveSystem.AIIDList> { }

[CustomPropertyDrawer(typeof(RPGCreationKit.CellsSystem.AllDoorsDictionary))]
public class MyDictionaryDrawer17 : DictionaryDrawer<string, RPGCreationKit.CellsSystem.Door> { }

[CustomPropertyDrawer(typeof(RPGCreationKit.PersistentReferences.CellsDoorsDictionary))]
public class MyDictionaryDrawer18 : DictionaryDrawer<string, string> { }

/*
[CustomPropertyDrawer(typeof(RPGCreationKit.SaveSystem.QuestIDQuestDataDictionary))]
public class MyDictionaryDrawer19 : QuestIDQuestDataDictionaryDrawer<string, RPGCreationKit.SaveSystem.QuestData> { }

*/

[CustomPropertyDrawer(typeof(RPGCreationKit.CellsSystem.MutablesInWorldDictionary))]
public class MyDictionaryDrawer20 : DictionaryDrawer<string, RPGCreationKit.Mutable> { }


[CustomPropertyDrawer(typeof(RPGCreationKit.CellsSystem.AllActionPointsDictionary))]
public class MyDictionaryDrawer21 : DictionaryDrawer<string, RPGCreationKit.AI.NPCActionPoint> { }

[CustomPropertyDrawer(typeof(RPGCreationKit.RacesDatabaseDictionary))]
public class MyDictionaryDrawer22 : DictionaryDrawer<string, RPGCreationKit.Race> { }


[CustomPropertyDrawer(typeof(RPGCreationKit.CellsSystem.TargetableTransformInWorldDictionary))]
public class MyDictionaryDrawer23 : DictionaryDrawer<string, RPGCreationKit.TargetableTransform> { }


[CustomPropertyDrawer(typeof(RPGCreationKit.CellsSystem.AIPathInWorldDictionary))]
public class MyDictionaryDrawer24 : DictionaryDrawer<string, RPGCreationKit.AI.AICustomPath> { }

