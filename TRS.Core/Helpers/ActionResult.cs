using System;
using System.Collections.Generic;
using System.Linq;

namespace TRS.Core.Helpers
{
    public class ActionResult
    {
        /// <summary>
        /// ctor of <see cref="ActionResult" />
        /// </summary>
        public ActionResult()
        {
            ErrorMessages = new List<string>();
            Exception = null;
        }

        public static ActionResult<TNew> From<TNew>(ActionResult otherResultToCopy, TNew data)
        {
            var newResult = new ActionResult<TNew>
            {
                IsSucceed = otherResultToCopy.IsSucceed,
                Data = data,
                ErrorMessages = otherResultToCopy.ErrorMessages,
                Exception = otherResultToCopy.Exception
            };
            return newResult;
        }

        /// <summary>
        /// Is Action Succeed ?
        /// </summary>
        /// <value><c>true</c> if this instance is succeed; otherwise, <c>false</c>.</value>
        public bool IsSucceed { get; set; }

        /// <summary>
        /// Is Action Failed ?
        /// </summary>
        /// <value><c>true</c> if this instance is failed; otherwise, <c>false</c>.</value>
        public bool IsFailed => !IsSucceed;

        /// <summary>
        /// Containing all Error Messages in <see cref="Exception" /> hierarchy
        /// </summary>
        /// <value>The error messages.</value>
        public List<string> ErrorMessages { get; set; }

        /// <summary>
        /// Gets or sets the exception.
        /// </summary>
        /// <value>The exception.</value>
        public Exception Exception { get; set; }

        /// <summary>
        /// Succeeds this instance.
        /// </summary>
        /// <returns>ActionResult.</returns>
        public static ActionResult Succeed()
        {
            return new ActionResult()
            {
                IsSucceed = true
            };
        }

        /// <summary>
        /// Failed the specified error message.
        /// </summary>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns>ActionResult.</returns>
        public static ActionResult Failed(params string[] errorMessages)
        {
            return new ActionResult()
            {
                IsSucceed = false,
                Exception = null,
                ErrorMessages = errorMessages.ToList()
            };
        }

        /// <summary>
        /// Returns Failed result of specified result and exception
        /// </summary>
        /// <param name="exc">The exc.</param>
        /// <returns>ActionResult.</returns>
        public static ActionResult Failed(Exception exc)
        {
            var result = new ActionResult { IsSucceed = false, Exception = exc };

            result.ErrorMessages.Add(exc.Message);
            return result;
        }

        public ActionResult ThrowIfFailed()
        {
            if (IsFailed)
            {
                throw new Exception(ErrorMessages.FirstOrDefault() ?? "Action Result Failed");
            }

            return this;
        }
    }

    public class ActionResult<T> : ActionResult
    {
        /// <summary>
        /// Action results Data
        /// </summary>
        /// <value>The data.</value>
        public T Data;
        /// <summary>
        /// Succeeds the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>ActionResult&lt;T&gt;.</returns>
        public static ActionResult<T> Succeed(T data)
        {
            return new ActionResult<T>
            {
                IsSucceed = true,
                Data = data
            };
        }
        public new static ActionResult<T> Failed(params string[] errorMessages)
        {
            return new ActionResult<T>()
            {
                IsSucceed = false,
                Exception = null,
                ErrorMessages = errorMessages.ToList()
            };
        }

        /// <summary>
        /// Returns Failed result with specified Exception
        /// </summary>
        /// <param name="exc">The exc.</param>
        /// <returns>ActionResult&lt;T&gt;.</returns>
        public new static ActionResult<T> Failed(Exception exc)
        {
            var result = new ActionResult<T> { IsSucceed = false, Exception = exc };

            result.ErrorMessages.Add(exc?.Message);
            return result;
        }

        /// <summary>
        /// Will throw exception when result of action is not succeed
        /// </summary>
        /// <returns>ActionResult&lt;T&gt;.</returns>
        /// <exception cref="Exception"></exception>
        public ActionResult<T> ThrowIfError(bool throwIfEmptyData = false)
        {
            if (Exception != null) throw Exception;
            if (ErrorMessages.Any()) throw new Exception(ErrorMessages.FirstOrDefault());
            if (throwIfEmptyData)
            {
                if (Data == null)
                {
                    throw new Exception("Data came here as null in ActionResult");
                }
            }
            return this;
        }
    }
}
