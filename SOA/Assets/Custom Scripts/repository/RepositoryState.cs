﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace soa
{
    public class RepositoryState
    {
        
        private readonly Dictionary<CacheKey, Hash> objects;
		private int revisionNumber;

        public RepositoryState(int revisionNumber)
        {
            this.objects = new Dictionary<CacheKey, Hash>(1000);
			this.revisionNumber = revisionNumber;
        }
        
        public IEnumerable<RepositoryObject> GetObjects()
        {
            List<RepositoryObject> repoObjects = new List<RepositoryObject>();
            foreach (KeyValuePair<CacheKey, Hash> entry in objects)
            {
                repoObjects.Add(new RepositoryObject(entry.Key, entry.Value));
            }
            return repoObjects;
        }
        public int Size()
        {
            return objects.Count;
        }
		public int RevisionNumber()
		{
			return revisionNumber;
		}

        public void Add(CacheKey key, Hash hash)
        {
            objects[key] = hash;
        }

        public Hash Find(CacheKey key)
        {
            Hash hash = null;
            objects.TryGetValue(key, out hash);
            return hash;
        }

        public ICollection<CacheKey> Diff(RepositoryState other)
        {
            ICollection<CacheKey> diffSet = new List<CacheKey>(Size());

            foreach (KeyValuePair<CacheKey, Hash> entry in objects)
            {
                CacheKey myKey = entry.Key;
                Hash myHash = entry.Value;
                Hash otherHash = other.Find(myKey);
                if (otherHash == null || !myHash.Equals(otherHash))
                {
                    diffSet.Add(myKey);
                }
            }

            return diffSet;
        }

        public class RepositoryObject
        {
            public CacheKey key;
            public Hash hash;

            public RepositoryObject(CacheKey key, Hash hash)
            {
                this.key = key;
                this.hash = hash;
            }

            public override int GetHashCode()
            {
               return key.GetHashCode() ^ hash.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                RepositoryObject other = obj as RepositoryObject;
                if (other == null)
                    return false;

                return key.Equals(other.key)
                        && hash.Equals(other.hash);
            }
        }
        
    }
}
