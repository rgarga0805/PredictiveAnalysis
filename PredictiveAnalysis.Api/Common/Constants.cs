using System.Collections.Generic;

namespace PredictiveAnalysis.Common
{
    public static class CacheKeys
    {
        public const string IndependantVariables = "IndependantVariables";
        public const string OutputVariable = "OutputVariable";
        public const string Rule = "Rule";
    }

    public static class Literals
    {
        public const string Space = " ";
        public const string Ampersand = "&";
        public const string At = "@";
        public const string Comma = ",";
        public const string Equal = "=";
        public const string Question = "?";
        public const string JPathPoliciesLob = "$..Policies[?(@.Name=='LOB')]";
        public const string JPathPoliciesOffice = "$..Policies[?(@.Name=='Office')]";
        public const string JPathPoliciesAction = "$..Policies[?(@.Name=='Actions')]";
    }
}

internal static class ServiceParameters
{
    internal const string ContentTypeJson = "application/json";
    internal const string ResponseTypeJson = "application/json";
    internal const string QueryStringStaffLoginId = "loginId";
    internal const string QueryStringApplicationId = "ApplicationId";
}

internal static class ServiceResponse
{
    public const int Success = 0;
}