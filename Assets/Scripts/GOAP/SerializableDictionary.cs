/*
 * SerializableDictionary.cs
 * --------------------------
 * This script provides a generic, serializable dictionary implementation for Unity,
 * overcoming Unity's native limitation that prevents generic dictionaries from being serialized.
 *
 * Tasks:
 *  - The SerializableKeyValuePair<TKey, TValue> class defines a serializable key-value pair.
 *  - The SerializableDictionary<TKey, TValue> class:
 *      - Stores data as both a standard Dictionary<TKey, TValue> (runtime) and a List<SerializableKeyValuePair<TKey, TValue>> (for Unity serialization).
 *      - Implements Unity’s ISerializationCallbackReceiver to synchronize between the two representations before/after (de)serialization.
 *      - Implements IEnumerable<SerializableKeyValuePair<TKey, TValue>> to support foreach iteration.
 *
 * Extras:
 *  - Provides clone functionality and conversion back to Dictionary.
 *  - Includes safety with TryGetValue, ContainsKey, and graceful serialization lifecycle handling.
 *  - Adds helper methods such as GetLivePairs() to expose runtime dictionary content.
 *
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

[Serializable]
public class SerializableDictionary<TKey, TValue> : ISerializationCallbackReceiver, IEnumerable<SerializableKeyValuePair<TKey, TValue>>
{
    // Used to be serialized in Unity
    [SerializeField]
    private List<SerializableKeyValuePair<TKey, TValue>> keyValuePairs = new();
    // Used at runtime
    private Dictionary<TKey, TValue> dictionary = new();

    // Indexer for dictionary access
    public TValue this[TKey key]
    {
        get => dictionary[key];
        set => dictionary[key] = value;
    }

    // Default Constructor
    public SerializableDictionary()
    {
        dictionary = new Dictionary<TKey, TValue>();
        keyValuePairs = new List<SerializableKeyValuePair<TKey, TValue>>();
    }

    // Overloaded Constructor
    public SerializableDictionary(IDictionary<TKey, TValue> _other)
    {
        dictionary = new Dictionary<TKey, TValue>(_other);
        keyValuePairs = new List<SerializableKeyValuePair<TKey, TValue>>();
        foreach (var kvp in dictionary)
        {
            keyValuePairs.Add(new SerializableKeyValuePair<TKey, TValue>(kvp.Key, kvp.Value));
        }
    }

    /* 
     * Clone() creates a new deep copy of the dictionary.
     */
    public SerializableDictionary<TKey, TValue> Clone() => new SerializableDictionary<TKey, TValue>(this.ToDictionary());

    /* 
     * ToDictionary() converts this dictionary to a standard dictionary.
     */
    public Dictionary<TKey, TValue> ToDictionary() => new Dictionary<TKey, TValue>(dictionary);

    /* 
     * ContainsKey() checks if a key exists.
     */
    public bool ContainsKey(TKey key) => dictionary.ContainsKey(key);

    /* 
     * Add() adds a new dictionary entry and key-value pair.
     */
    public void Add(TKey key, TValue value)
    {
        dictionary.Add(key, value);
        keyValuePairs.Add(new SerializableKeyValuePair<TKey, TValue>(key, value));
    }

    /* 
     * Remove() removes a dictionary entry by key and also key-value pair from both run time
     * and serialized data.
     */
    public bool Remove(TKey key)
    {
        if (dictionary.Remove(key))
        {
            keyValuePairs.RemoveAll(pair => EqualityComparer<TKey>.Default.Equals(pair.Key, key));
            return true;
        }
        return false;
    }

    /* 
     * TryGetValue() is a safe getter for returning a value from a specific key-value pair.
     */
    public bool TryGetValue(TKey key, out TValue value) => dictionary.TryGetValue(key, out value);

    /* 
     * GetLivePairs() returns a live runtime dictionary (Useful in foreach loops).
     */
    public IEnumerable<KeyValuePair<TKey, TValue>> GetLivePairs() => dictionary;

    /* 
     * OnBeforeSerialize() clears key-value pairs and loops through the dictionary to 
     * add the pairs before Unity serializes in the editor.
     */
    public void OnBeforeSerialize()
    {
        keyValuePairs.Clear();
        foreach (var pair in dictionary)
        {
            keyValuePairs.Add(new SerializableKeyValuePair<TKey, TValue>(pair.Key, pair.Value));
        }
    }

    /* 
     * OnAfterDeSerialize() clears the dictionary and loops through the key-value pairs 
     * to set the key and value in the dictionary after Unity deserializes in the editor.
     */
    public void OnAfterDeserialize()
    {
        dictionary.Clear();
        foreach (var pair in keyValuePairs)
        {
            dictionary[pair.Key] = pair.Value;
        }
    }

    // Iterators
    public IEnumerator<SerializableKeyValuePair<TKey, TValue>> GetEnumerator() => keyValuePairs.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

}
