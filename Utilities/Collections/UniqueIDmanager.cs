using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Tools
{
    /// <summary>
    /// </summary>
    public interface IUniqueInstanced
    {
        int UniqueID { get; set; }
    }
    /// <summary>
    /// Generate a unique int32 id
    /// </summary>
    public class UniqueIndexManager
    {
        /// <summary>
        /// Maximum indices count
        /// </summary>
        public readonly int Capacity = 0;

        BitArray1 indexfield;

        /// <summary>
        /// Using a bitarray with integers, the size in byte of this class is a multiple of 32 indices, example all initialization
        /// from maximum index = 0 to maximum index = 31 have the same size in bytes.
        /// </summary>
        public UniqueIndexManager(int Capacity, bool initial)
        {
            this.Capacity = Capacity;
            indexfield = new BitArray1(Capacity, initial);
        }

        /// <summary>
        /// The current maximum index used, can be used example to optimize the searching of new available index
        /// </summary>
        public int MaxIndexUsed { get; private set; }

        /// <summary>
        /// Return the position of first not-set index, it's the "0" in bitarray
        /// Return -1 if there aren't available indices
        /// </summary>
        /// <param name="istart">the first index to test</param>
        public int GetNextAvailableIndex(int istart = 0)
        {
            return indexfield.SearchNext(istart, false);
            //return indexfield.SearchNext_old(istart, false);
        }
        /// <summary>
        /// Return the position of first used index, it's the "1" in bitarray
        /// Return -1 if there aren't available indices
        /// </summary>
        /// <param name="istart">the first index to test</param>
        public int GetNextInUseIndex(int istart = 0)
        {
            return indexfield.SearchNext(istart, true);
        }

        /// <summary>
        /// Set the index value to USED or NOT-USED
        /// </summary>
        public void SetIndex(int index, bool inUse)
        {
            indexfield[index] = inUse;

            if (inUse && MaxIndexUsed < index) MaxIndexUsed = index;
        }

        /// <summary>
        /// Return true if index are used, false if is avaiable
        /// </summary>
        public bool GetIndex(int index)
        {
            return indexfield[index];
        }

        /// <summary>
        /// Find the correct "MaxIndeUsed" when you have removed a lot of indices at the end
        /// </summary>
        public void TrimIndices()
        {
            int i = MaxIndexUsed;
            while (!indexfield[i] && i >= 0) { i--; MaxIndexUsed--; }
        }

        /// <summary>
        /// print to string the bitarray in compact mode, show all indices used
        /// </summary>
        public override string ToString()
        {
            return indexfield.ToCompactString();
        }
    }
}
