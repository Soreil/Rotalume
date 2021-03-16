using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace WPFFrontend
{
    public class KeyBoardWithInterruptHandler
    {
        public event EventHandler<EventArgs>? KeyWentDown;

        private readonly Dictionary<Key, bool> dictionary = new();

        private readonly HashSet<Key> MappedKeys;
        public KeyBoardWithInterruptHandler(HashSet<Key> mappedKeys)
        {
            MappedKeys = mappedKeys;
            foreach (var k in MappedKeys)
            {
                dictionary.Add(k, false);
            }
        }

        public bool this[Key contains] => MappedKeys.Contains(contains) && dictionary[contains];

        public void Down(object? sender, KeyEventArgs e)
        {
            if (dictionary.ContainsKey(e.Key)) dictionary[e.Key] = true;

            if (MappedKeys.Contains(e.Key))
                OnAnyKeyDown(EventArgs.Empty);
        }

        public void Up(object? sender, KeyEventArgs e)
        {
            if (dictionary.ContainsKey(e.Key)) dictionary[e.Key] = false;
        }

        protected virtual void OnAnyKeyDown(EventArgs e) => KeyWentDown?.Invoke(this, e);
    }
}