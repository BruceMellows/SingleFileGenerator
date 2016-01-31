// There is no copyright, you can use and abuse this source without limit.
// There is no warranty, you are responsible for the consequences of your use of this source.
// There is no burden, you do not need to acknowledge this source in your use of this source.

namespace BadCompany.SingleFileGeneratorInterface
{
    public interface IProvider
    {
        /// <summary>
        /// Writes the output.
        /// </summary>
        /// <param name="inputFilePath">The input file path.</param>
        /// <param name="inputFileContent">Content of the input file.</param>
        /// <param name="defaultNamespace">The default namespace.</param>
        /// <param name="defaultClassName">Default name of the class.</param>
        /// <param name="codeProvider">The code provider.</param>
        /// <param name="writer">The writer.</param>
        void WriteOutput(
            string inputFilePath,
            string inputFileContent,
            string defaultNamespace,
            string defaultClassName,
            System.CodeDom.Compiler.CodeDomProvider codeProvider,
            System.IO.StreamWriter writer);
    }

    public static class GeneratorProviderExtensions
    {
        /// <summary>
        /// Writes the output.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="inputFilePath">The input file path.</param>
        /// <param name="defaultNamespace">The default namespace.</param>
        /// <param name="defaultClassName">Default name of the class.</param>
        /// <param name="codeProvider">The code provider.</param>
        /// <param name="writer">The writer.</param>
        public static void WriteOutput(
            this IProvider provider,
            string inputFilePath,
            string defaultNamespace,
            string defaultClassName,
            System.CodeDom.Compiler.CodeDomProvider codeProvider,
            System.IO.StreamWriter writer)
        {
            provider.WriteOutput(
                inputFilePath,
                System.IO.File.ReadAllText(inputFilePath),
                defaultNamespace,
                defaultClassName,
                codeProvider,
                writer);
        }

        /// <summary>
        /// Gets the output.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="inputFilePath">The input file path.</param>
        /// <param name="inputFileContent">Content of the input file.</param>
        /// <param name="defaultNamespace">The default namespace.</param>
        /// <param name="defaultClassName">Default name of the class.</param>
        /// <param name="codeProvider">The code provider.</param>
        /// <returns></returns>
        public static string GetOutput(
            this IProvider provider,
            string inputFilePath,
            string inputFileContent,
            string defaultNamespace,
            string defaultClassName,
            System.CodeDom.Compiler.CodeDomProvider codeProvider)
        {
            System.IO.MemoryStream memoryStream = null;
            System.IO.StreamWriter streamWriter = null;
            try
            {
                memoryStream = new System.IO.MemoryStream();
                streamWriter = new System.IO.StreamWriter(memoryStream);

                provider.WriteOutput(
                    inputFilePath,
                    inputFileContent,
                    defaultNamespace,
                    defaultClassName,
                    codeProvider,
                    streamWriter);

                var result = System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
                return result;
            }
            finally
            {
                if (streamWriter != null)
                {
                    streamWriter.Dispose();
                }

                if (memoryStream != null)
                {
                    memoryStream.Dispose();
                }
            }
        }

        /// <summary>
        /// Gets the output.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="inputFilePath">The input file path.</param>
        /// <param name="defaultNamespace">The default namespace.</param>
        /// <param name="defaultClassName">Default name of the class.</param>
        /// <param name="codeProvider">The code provider.</param>
        /// <returns></returns>
        public static string GetOutput(
            this IProvider provider,
            string inputFilePath,
            string defaultNamespace,
            string defaultClassName,
            System.CodeDom.Compiler.CodeDomProvider codeProvider)
        {
            return provider.GetOutput(
                inputFilePath,
                System.IO.File.ReadAllText(inputFilePath),
                defaultNamespace,
                defaultClassName,
                codeProvider);
        }
    }
}