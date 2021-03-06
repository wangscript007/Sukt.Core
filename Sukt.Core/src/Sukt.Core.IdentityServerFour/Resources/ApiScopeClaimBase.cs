﻿using System;
using System.ComponentModel;

namespace Sukt.Core.IdentityServerFour
{
    /// <summary>
    /// api授权范围声明
    /// </summary>
    [DisplayName("api授权范围声明")]
    public abstract class ApiScopeClaimBase : UserClaimBase
    {
        /// <summary>
        /// 范围id
        /// </summary>
        [DisplayName("范围id")]
        public Guid ScopeId { get; set; }

        ///// <summary>
        ///// 范围
        ///// </summary>
        //[DisplayName("范围")]
        //public ApiScopeBase Scope { get; set; }
    }
}