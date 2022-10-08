namespace NetSimul.Component
{
    public interface IErrorDetectionAlg
    {
        /// <summary>
        /// Genera el código de verificación correspondiente a esos datos
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
        string GetVerificationData(string data);
        /// <summary>
        /// Retorna el string data si no se detectaron errores, retorna un string distinto a data si se corrigió el valor y retorna null en el caso 
        /// en el que la entrada estaba dañada y no pudo corregirla 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="verificationData"></param>
        /// <returns></returns>
        string DetectNoise(string data, string verificationData);
    }
}



