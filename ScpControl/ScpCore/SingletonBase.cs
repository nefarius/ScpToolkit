using System;
using System.Runtime.Serialization;

namespace ScpControl.ScpCore
{
    /// <summary>
    ///     A base class for the singleton design pattern.
    /// </summary>
    /// <typeparam name="T">Class type of the singleton</typeparam>
    [DataContract]
    public abstract class SingletonBase<T> where T : class
    {
        #region Members

        /// <summary>
        ///     Static instance. Needs to use lambda expression
        ///     to construct an instance (since constructor is private).
        /// </summary>
        private static readonly Lazy<T> SInstance = new Lazy<T>(CreateInstanceOfT);

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the instance of this singleton.
        /// </summary>
        public static T Instance
        {
            get { return SInstance.Value; }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Creates an instance of T via reflection since T's constructor is expected to be private.
        /// </summary>
        /// <returns></returns>
        private static T CreateInstanceOfT()
        {
            return Activator.CreateInstance(typeof (T), true) as T;
        }

        #endregion
    }
}