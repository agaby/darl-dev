// ***********************************************************************
// Assembly         : DarlCompiler.Interpreter
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="SpecialFormBinding.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
namespace DarlCompiler.Interpreter
{


    /// <summary>
    /// Class SpecialFormBindingInfo.
    /// </summary>
    public class SpecialFormBindingInfo : BindingTargetInfo, IBindingSource
    {
        /// <summary>
        /// The binding
        /// </summary>
        public readonly ConstantBinding Binding;
        /// <summary>
        /// The minimum child count
        /// </summary>
        public readonly int MinChildCount, MaxChildCount;
        /// <summary>
        /// The child roles
        /// </summary>
        public string[] ChildRoles;
        /// <summary>
        /// Initializes a new instance of the <see cref="SpecialFormBindingInfo"/> class.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="form">The form.</param>
        /// <param name="minChildCount">The minimum child count.</param>
        /// <param name="maxChildCount">The maximum child count.</param>
        /// <param name="childRoles">The child roles.</param>
        public SpecialFormBindingInfo(string symbol, SpecialForm form, int minChildCount = 0, int maxChildCount = 0, string childRoles = null)
            : base(symbol, BindingTargetType.SpecialForm)
        {
            Binding = new ConstantBinding(form, this);
            MinChildCount = minChildCount;
            MaxChildCount = Math.Max(minChildCount, maxChildCount); //if maxParamCount=0 then set it equal to minParamCount
            if (!string.IsNullOrEmpty(childRoles))
            {
                ChildRoles = childRoles.Split(',');
            }
        }

        #region IBindingSource Members

        /// <summary>
        /// Binds the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>Binding.</returns>
        public Binding Bind(BindingRequest request)
        {
            return Binding;
        }

        #endregion
    }

    /// <summary>
    /// Class BindingSourceTableExtensions.
    /// </summary>
    public static partial class BindingSourceTableExtensions
    {
        //constructor  for adding methods to BuiltIns table in Runtime
        /// <summary>
        /// Adds the special form.
        /// </summary>
        /// <param name="targets">The targets.</param>
        /// <param name="form">The form.</param>
        /// <param name="formName">Name of the form.</param>
        /// <param name="minChildCount">The minimum child count.</param>
        /// <param name="maxChildCount">The maximum child count.</param>
        /// <param name="parameterNames">The parameter names.</param>
        /// <returns>BindingTargetInfo.</returns>
        public static BindingTargetInfo AddSpecialForm(this BindingSourceTable targets, SpecialForm form, string formName,
                        int minChildCount = 0, int maxChildCount = 0, string parameterNames = null)
        {
            var formInfo = new SpecialFormBindingInfo(formName, form, minChildCount, maxChildCount, parameterNames);
            targets.Add(formName, formInfo);
            return formInfo;
        }

    }

} 
