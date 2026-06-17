using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace O2un.Data 
{
    public sealed class DataProvider
    {
        private readonly Dictionary<Type, object> _cache = new();
        private readonly HashSet<Type> _dirty = new();

        private static string GetFilePaht<T>() => Path.Combine(Application.persistentDataPath, $"{typeof(T).Name}.json");

        public async UniTask<T> Load<T>() where T : new()
        {
            if(_cache.TryGetValue(typeof(T), out object cached))
            {
                return (T)cached;
            }

            string path = GetFilePaht<T>();
            T data = File.Exists(path) ? JsonUtility.FromJson<T>(await File.ReadAllTextAsync(path)) : new();
            _cache[typeof(T)] = data;
            return data;
        }

        public void Save<T>(T data)
        {
            _cache[typeof(T)] = data;
            _dirty.Add(typeof(T));
        }

        public async UniTaskVoid Flush<T>()
        {
            if(!_dirty.Contains(typeof(T)))
            {
                return;
            }

            if(_cache.TryGetValue(typeof(T), out object cached))
            {
                await File.WriteAllTextAsync(GetFilePaht<T>(), JsonUtility.ToJson((T)cached));
            }
            _dirty.Remove(typeof(T));
        }

        public async UniTaskVoid Flush()
        {
            foreach(var type in _dirty.ToList())
            {
                if(_cache.TryGetValue(type, out object data))
                {
                    string path = Path.Combine(Application.persistentDataPath, $"{type.Name}.json");
                    await File.WriteAllTextAsync(path, JsonUtility.ToJson(data));
                }
            }
            _dirty.Clear();
        }
    }
}
