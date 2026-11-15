using System;
using System.Collections.Generic;

namespace HomeFitPlanner.Core.DataStructures.Trees;

/// <summary>
/// Hand-written Prefix Tree (Trie) for real-time username and email indexing.
/// Guarantees O(L) time complexity for search, insertion, and prefix matching,
/// where L is the length of the query string, completely independent of total users (N).
/// </summary>
public class Trie
{
    private class TrieNode
    {
        public char Character { get; set; }
        public bool IsEndOfWord { get; set; }
        public Dictionary<char, TrieNode> Children { get; } = new();
        public int SubtreeWordCount { get; set; }

        public TrieNode(char c = '\0')
        {
            Character = c;
        }
    }

    private readonly TrieNode _root = new('\0');
    private int _wordCount;

    public int WordCount => _wordCount;

    public void Insert(string word)
    {
        if (string.IsNullOrWhiteSpace(word)) return;
        word = word.Trim().ToLowerInvariant();

        var current = _root;
        current.SubtreeWordCount++;

        foreach (char c in word)
        {
            if (!current.Children.TryGetValue(c, out var next))
            {
                next = new TrieNode(c);
                current.Children[c] = next;
            }
            current = next;
            current.SubtreeWordCount++;
        }

        if (!current.IsEndOfWord)
        {
            current.IsEndOfWord = true;
            _wordCount++;
        }
    }

    public bool Contains(string word, out int depthTraversed)
    {
        depthTraversed = 0;
        if (string.IsNullOrWhiteSpace(word)) return false;
        word = word.Trim().ToLowerInvariant();

        var current = _root;
        foreach (char c in word)
        {
            depthTraversed++;
            if (!current.Children.TryGetValue(c, out var next))
            {
                return false;
            }
            current = next;
        }

        return current.IsEndOfWord;
    }

    public bool StartsWith(string prefix, out int depthTraversed)
    {
        depthTraversed = 0;
        if (string.IsNullOrWhiteSpace(prefix)) return false;
        prefix = prefix.Trim().ToLowerInvariant();

        var current = _root;
        foreach (char c in prefix)
        {
            depthTraversed++;
            if (!current.Children.TryGetValue(c, out var next))
            {
                return false;
            }
            current = next;
        }

        return true;
    }
}

