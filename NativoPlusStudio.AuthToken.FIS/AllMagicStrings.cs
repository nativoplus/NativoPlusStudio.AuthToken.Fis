namespace NativoPlusStudio.AuthToken.FIS
{
    public static class AllMagicStrings
    {
        public static string AuthCommand { get; set; } = @"SoapCommands/AuthCommand.txt";
        public static string TokenNodeInFisAuthRequest { get; set; } = @"//chexAuthenticateReturn";
        public static string TokenNodeInFisRequest { get; set; } = @"//in0";
        public static string ErrorMessageNodeInFisResponse { get; set; } = @"//faultstring";
    }
}
