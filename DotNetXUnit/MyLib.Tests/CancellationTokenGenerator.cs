﻿using AutoFixture.Kernel;

namespace MyLib.Tests;

public class CancellationTokenGenerator : ISpecimenBuilder
{
    public object Create(object request, ISpecimenContext context)
    {
        if (request is Type type)
        {
            return type == typeof(CancellationToken) ? new CancellationTokenSource().Token : new NoSpecimen();
        }
        else
        {
            return new NoSpecimen();
        }
    }
}
