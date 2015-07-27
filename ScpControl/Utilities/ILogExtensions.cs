using System;
using System.Diagnostics;
using log4net;

namespace ScpControl.Utilities
{
    public static class FuncExtenstion
    {
        [DebuggerStepThrough]
        public static Action<A> AsAction<A, T>(this Func<A, T> f)
        {
            return (a) => f(a);
        }

        [DebuggerStepThrough]
        public static Func<bool> AsFunc(this Action a)
        {
            return () => { a(); return true; };
        }
    }

    public static class IlogExtensions
    {
        public static TResult TryCatchLogThrow<TResult>(this ILog logger, Func<TResult> f)
        {
            try
            {
                return f();
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                throw;
            }
        }

        public static TResult TryCatchSilent<TResult>(this ILog logger, Func<TResult> f)
        {
            try
            {
                return f();
            }
            catch
            {
                return default(TResult);
            }
        }

        public static void TryCatchSilent(this ILog logger, Action f)
        {
            logger.TryCatchSilent(f.AsFunc());
        }

        public static void TryCatchLogThrow(this ILog logger, Action f)
        {
            logger.TryCatchLogThrow(f.AsFunc());
        }

        public static TResult TryCatchLog<TResult>(this ILog logger, Func<TResult> f)
        {
            try
            {
                return f();
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                return default(TResult);
            }
        }

        public static void TryCatchLog(this ILog logger, Action f)
        {
            logger.TryCatchLog(f.AsFunc());
        }
    }
}
