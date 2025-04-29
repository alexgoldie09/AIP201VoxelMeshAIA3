using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Serializable key-value pair for Unity
/// </summary>
[Serializable]
public class SerializableKeyValuePair<TKey, TValue>
{
    public TKey Key;
    public TValue Value;

    public SerializableKeyValuePair(TKey key, TValue value)
    {
        Key = key;
        Value = value;
    }
}

/// <summary>
/// A serializable dictionary wrapper for Unity
/// </summary>
[Serializable]
public class SerializableDictionary<TKey, TValue> : ISerializationCallbackReceiver, IEnumerable<SerializableKeyValuePair<TKey, TValue>>
{
    [SerializeField]
    private List<SerializableKeyValuePair<TKey, TValue>> keyValuePairs = new();

    private Dictionary<TKey, TValue> dictionary = new();

    public TValue this[TKey key]
    {
        get => dictionary[key];
        set => dictionary[key] = value;
    }

    public SerializableDictionary()
    {
        dictionary = new Dictionary<TKey, TValue>();
        keyValuePairs = new List<SerializableKeyValuePair<TKey, TValue>>();
    }

    public SerializableDictionary(IDictionary<TKey, TValue> _other)
    {
        dictionary = new Dictionary<TKey, TValue>(_other);
        keyValuePairs = new List<SerializableKeyValuePair<TKey, TValue>>();
        foreach (var kvp in dictionary)
        {
            keyValuePairs.Add(new SerializableKeyValuePair<TKey, TValue>(kvp.Key, kvp.Value));
        }
    }

    public SerializableDictionary<TKey, TValue> Clone()
    {
        return new SerializableDictionary<TKey, TValue>(this.ToDictionary());
    }


    public Dictionary<TKey, TValue> ToDictionary() => new Dictionary<TKey, TValue>(dictionary);

    public bool ContainsKey(TKey key) => dictionary.ContainsKey(key);

    public void Add(TKey key, TValue value)
    {
        dictionary.Add(key, value);
        keyValuePairs.Add(new SerializableKeyValuePair<TKey, TValue>(key, value));
    }

    public bool Remove(TKey key)
    {
        if (dictionary.Remove(key))
        {
            // Also remove from the serialized list
            keyValuePairs.RemoveAll(pair => EqualityComparer<TKey>.Default.Equals(pair.Key, key));
            return true;
        }
        return false;
    }


    public bool TryGetValue(TKey key, out TValue value) => dictionary.TryGetValue(key, out value);

    public IEnumerable<KeyValuePair<TKey, TValue>> GetLivePairs()
    {
        return dictionary;
    }

    public void OnBeforeSerialize()
    {
        keyValuePairs.Clear();
        foreach (var pair in dictionary)
        {
            keyValuePairs.Add(new SerializableKeyValuePair<TKey, TValue>(pair.Key, pair.Value));
        }
    }

    public void OnAfterDeserialize()
    {
        dictionary.Clear();
        foreach (var pair in keyValuePairs)
        {
            dictionary[pair.Key] = pair.Value;
        }
    }


    public IEnumerator<SerializableKeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return keyValuePairs.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

}
