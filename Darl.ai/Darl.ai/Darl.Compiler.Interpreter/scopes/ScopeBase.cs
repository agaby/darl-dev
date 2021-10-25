// ***********************************************************************
// Assembly         : DarlCompiler.Interpreter
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="ScopeBase.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Threading;

namespace DarlCompiler.Interpreter
{

    /// <summary>
    /// Class ScopeBase.
    /// </summary>
    public class ScopeBase
    {
        /// <summary>
        /// The information
        /// </summary>
        public ScopeInfo Info;
        /// <summary>
        /// The values
        /// </summary>
        public volatile object[] Values;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScopeBase"/> class.
        /// </summary>
        /// <param name="scopeInfo">The scope information.</param>
        public ScopeBase(ScopeInfo scopeInfo) : this(scopeInfo, null) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="ScopeBase"/> class.
        /// </summary>
        /// <param name="scopeInfo">The scope information.</param>
        /// <param name="values">The values.</param>
        public ScopeBase(ScopeInfo scopeInfo, object[] values)
        {
            Info = scopeInfo;
            Values = values;
            if (Values == null)
                Values = new object[scopeInfo.ValuesCount];
        }

        /// <summary>
        /// Adds the slot.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>SlotInfo.</returns>
        public SlotInfo AddSlot(string name)
        {
            var slot = Info.AddSlot(name, SlotType.Value);
            if (slot.Index >= Values.Length)
                Resize(Values.Length + 4);
            return slot;
        }

        /// <summary>
        /// Gets the values.
        /// </summary>
        /// <returns>System.Object[].</returns>
        public object[] GetValues()
        {
            return Values;
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>System.Object.</returns>
        public object GetValue(int index)
        {
            try
            {
                var tmp = Values;
                // The following line may throw null-reference exception (tmp==null), if resizing is happening at the same time
                // It may also throw IndexOutOfRange exception if new variable was added by another thread in another frame(scope)
                // but this scope and Values array were created before that, so Values is shorter than #slots in SlotInfo. 
                // But in this case, it does not matter, result value is null (unassigned)
                return tmp[index];
            }
            catch (NullReferenceException)
            {
                Thread.Sleep(0); // Silverlight does not have Thread.Yield; 
                // Thread.Yield(); // maybe SpinWait.SpinOnce?
                return GetValue(index); //repeat attempt
            }
            catch (IndexOutOfRangeException)
            {
                return null; //we do not resize here, value is unassigned anyway.
            }

        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        public void SetValue(int index, object value)
        {
            try
            {
                var tmp = Values;
                // The following line may throw null-reference exception (tmp==null), if resizing is happening at the same time
                // It may also throw IndexOutOfRange exception if new variable was added by another thread in another frame(scope)
                // but this scope and Values array were created before that, so Values is shorter than #slots in SlotInfo 
                tmp[index] = value;
                //Now check that tmp is the same as Values - if not, then resizing happened in the middle, 
                // so repeat assignment to make sure the value is in resized array.
                if (tmp != Values)
                    SetValue(index, value); // do it again
            }
            catch (NullReferenceException)
            {
                Thread.Sleep(0); // it's  OK to Sleep intead of SpinWait - it is really rare event, so we don't care losing a few more cycles here. 
                SetValue(index, value); //repeat it again
            }
            catch (IndexOutOfRangeException)
            {
                Resize(Info.GetSlotCount());
                SetValue(index, value); //repeat it again
            }
        }

        // Disabling warning: 'Values: a reference to a volatile field will not be treated as volatile'
        // According to MSDN for CS0420 warning (see http://msdn.microsoft.com/en-us/library/4bw5ewxy.aspx),
        // this does NOT apply to Interlocked API - which we use here.
#pragma warning disable 0420
        /// <summary>
        /// Resizes the specified new size.
        /// </summary>
        /// <param name="newSize">The new size.</param>
        protected void Resize(int newSize)
        {
            lock (this.Info.LockObject)
            {
                if (Values.Length >= newSize) return;
                object[] tmp = Interlocked.Exchange(ref Values, null);
                Array.Resize(ref tmp, newSize);
                Interlocked.Exchange(ref Values, tmp);
            }
        }

        /// <summary>
        /// Ases the dictionary.
        /// </summary>
        /// <returns>IDictionary&lt;System.String, System.Object&gt;.</returns>
        public IDictionary<string, object> AsDictionary()
        {
            return new ScopeValuesDictionary(this);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return Info.ToString();
        }


    }


}
