namespace NativoPlusStudio.AuthToken.FIS
{
    internal static class AllMagicStrings
    {
        public static string TokenNodeInFisAuthRequest { get; set; } = @"//chexAuthenticateReturn";
        public static string TokenNodeInFisRequest { get; set; } = @"//in0";
        public static string ErrorMessageNodeInFisResponse { get; set; } = @"//faultstring";

        public static string AuthCommand { get; set; } 
            = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:urn=""urn:fischex"">
    <soapenv:Header/>
    <soapenv:Body>
        <urn:chexAuthenticate>
            <in0>{{UserName}}</in0>
            <in1>{{Password}}</in1>
        </urn:chexAuthenticate>
    </soapenv:Body>
</soapenv:Envelope>";
    }
}
