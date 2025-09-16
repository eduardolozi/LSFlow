﻿using System.ComponentModel;

namespace LSFlow.IntegrationTests.Utils;

public static class EnumUtils
{
    public static string GetDescription(this Enum enumValue)
    {
        var field = enumValue.GetType().GetField(enumValue.ToString());
        if (field == null)
            return enumValue.ToString();

        if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
            return attribute.Description;

        return enumValue.ToString();
    }
}