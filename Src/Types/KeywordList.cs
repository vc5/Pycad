using AutocompleteMenuNS;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

namespace NFox.Pycad.Types
{
    class KeywordList: IEnumerable<AutocompleteItem>
    {

        public List<Statement> Statements { get; private set; }

        public List<Variant> Builtins { get; private set; }

        public List<Variant> Variants { get; private set; }

        public List<Variant> Properties { get; private set; }

        public KeywordList()
        {
            Clear();
        }

        public void Clear()
        {
            Statements = new List<Statement>();
            Builtins = new List<Variant>();
            Variants = new List<Variant>();
            Properties = new List<Variant>();
        }

        public void AddStatement(string key)
        {
            Statements.Add(new Statement(key));
        }

        public void AddStatement(Statement item)
        {
            Statements.Add(item);
        }

        public void AddBuiltin(string key, object obj)
        {
            if (obj != null)
                AddBuiltin(Variant.GetValue(key, obj));
        }

        public void AddBuiltin(Variant item)
        {
            Builtins.Add(item);
        }

        public void AddVariant(string key, object obj)
        {
            if (obj != null)
                AddVariant(new Variant(key, obj));
        }

        public void AddVariant(Variant item)
        {
            Variants.Add(item);
        }

        public void SetVariants(IEnumerable<Variant> items)
        {
            Variants = items.ToList();
        }

        public void AddProperty(string key, object obj)
        {
            if (obj != null)
                AddProperty(Variant.GetValue(key, obj));
        }

        public void AddProperty(Variant item)
        {
            Properties.Add(item);
        }

        public void SetProperties(IEnumerable<Variant> items)
        {
            Properties = items.ToList();
        }

        public Variant GetValue(string word)
        {
            var names = word.Split('.');
            var t = Builtins.First(w => w.Name == names[0]);
            if(t == null)
                t= Variants.First(w => w.Name == names[0]);
            for (int i = 1; i < names.Length; i++)
                t = t?.GetItem(names[i]);
            return t;
        }

        public IEnumerator<AutocompleteItem> GetEnumerator()
        {
            foreach (var item in Statements)
                yield return new TopItem(item);
            foreach (var item in Builtins)
                yield return new TopItem(item);
            foreach (var item in Variants)
                yield return new TopItem(item);
            foreach (var item in Properties.OrderBy(i => i))
                yield return new SubItem(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }
}
