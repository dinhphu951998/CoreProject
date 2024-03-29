﻿using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpProjectCore.Core.Infrastructure
{
    public class BaseEntity
    {
        public TDestination ToViewModel<TDestination>() where TDestination : BaseViewModel
        {
            return AutoMapperConfiguration.GetInstance().Map<TDestination>(this);
        }

    }
}
