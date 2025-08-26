// Copyright (c) 2025 Amin Hasanloo

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AHL.AnalyticsLite
{
    internal class FileEventQueue
    {
        readonly string _path;
        readonly object _lock = new object();
        readonly int _rotateAt;

        public FileEventQueue(string path, int rotateAt = 10000)
        {
            _path = path;
            _rotateAt = rotateAt <= 0 ? 10000 : rotateAt;
            Directory.CreateDirectory(Path.GetDirectoryName(_path));
            if (!File.Exists(_path))
                File.WriteAllText(_path, string.Empty, Encoding.UTF8);
        }

        public void Enqueue(string jsonLine)
        {
            lock (_lock)
            {
                File.AppendAllText(_path, jsonLine + "\n", Encoding.UTF8);
                TryRotate();
            }
        }

        public List<string> PeekBatch(int maxCount)
        {
            lock (_lock)
            {
                if (!File.Exists(_path)) return new List<string>();
                var lines = new List<string>(maxCount);
                using (var fs = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var sr = new StreamReader(fs, Encoding.UTF8))
                {
                    string line;
                    while (lines.Count < maxCount && (line = sr.ReadLine()) != null)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                            lines.Add(line);
                    }
                }
                return lines;
            }
        }

        public void RemoveBatch(int count)
        {
            if (count <= 0) return;
            lock (_lock)
            {
                var tmp = _path + ".tmp";
                int removed = 0;

                using (var input = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var reader = new StreamReader(input, Encoding.UTF8))
                using (var output = new FileStream(tmp, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var writer = new StreamWriter(output, Encoding.UTF8))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        if (removed < count) { removed++; continue; }
                        writer.WriteLine(line);
                    }
                }

                File.Delete(_path);
                File.Move(tmp, _path);
            }
        }

        public int CountApprox()
        {
            lock (_lock)
            {
                if (!File.Exists(_path)) return 0;
                int count = 0;
                using (var fs = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var sr = new StreamReader(fs, Encoding.UTF8))
                {
                    while (sr.ReadLine() != null) count++;
                }
                return count;
            }
        }

        void TryRotate()
        {
            try
            {
                int count = CountApprox();
                if (count > _rotateAt)
                {
                    var backup = _path + "." + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + ".bak";
                    File.Move(_path, backup);
                    File.WriteAllText(_path, string.Empty, Encoding.UTF8);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[AnalyticsLite] Rotate failed: {e.Message}");
            }
        }
    }
}
