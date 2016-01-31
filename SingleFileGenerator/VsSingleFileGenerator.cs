// There is no copyright, you can use and abuse this source without limit.
// There is no warranty, you are responsible for the consequences of your use of this source.
// There is no burden, you do not need to acknowledge this source in your use of this source.

/*
 * BASED ON: http://blogs.msdn.com/b/vsx/archive/2013/11/27/building-a-vsix-deployable-single-file-generator.aspx
 */

namespace BadCompany.SingleFileGenerator
{
    public class VsSingleFileGenerator<TProvider> : Microsoft.VisualStudio.Shell.Interop.IVsSingleFileGenerator, Microsoft.VisualStudio.OLE.Interop.IObjectWithSite
        where TProvider : SingleFileGeneratorInterface.IProvider, new()
    {
        #region Private

        private readonly SingleFileGeneratorInterface.IProvider provider;

        private object site;

        private System.CodeDom.Compiler.CodeDomProvider codeDomProvider;

        private Microsoft.VisualStudio.Shell.ServiceProvider serviceProvider;

        private System.CodeDom.Compiler.CodeDomProvider CodeProvider
        {
            get
            {
                return this.codeDomProvider
                       ?? (this.codeDomProvider =
                           (System.CodeDom.Compiler.CodeDomProvider)
                               ((Microsoft.VisualStudio.Designer.Interfaces.IVSMDCodeDomProvider)
                                   this.SiteServiceProvider.GetService(typeof(Microsoft.VisualStudio.Designer.Interfaces.IVSMDCodeDomProvider).GUID))
                                   .CodeDomProvider);
            }
        }

        private Microsoft.VisualStudio.Shell.ServiceProvider SiteServiceProvider
        {
            get
            {
                return this.serviceProvider
                       ?? (this.serviceProvider = new Microsoft.VisualStudio.Shell.ServiceProvider(this.site as Microsoft.VisualStudio.OLE.Interop.IServiceProvider));
            }
        }

        #endregion Private

        public VsSingleFileGenerator()
        {
            this.provider = new TProvider();
        }

        #region interface Microsoft.VisualStudio.Shell.Interop.IVsSingleFileGenerator

        public int DefaultExtension(out string pbstrDefaultExtension)
        {
            pbstrDefaultExtension = "." + this.CodeProvider.FileExtension;

            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int Generate(
            string wszInputFilePath,
            string bstrInputFileContents,
            string wszDefaultNamespace,
            System.IntPtr[] rgbOutputFileContents,
            out uint pcbOutput,
            Microsoft.VisualStudio.Shell.Interop.IVsGeneratorProgress pGenerateProgress)
        {
            var taskMem = System.IntPtr.Zero;

            try
            {
                string output;
                try
                {
                    output = SingleFileGeneratorInterface.GeneratorProviderExtensions.GetOutput(
                        this.provider,
                        wszInputFilePath,
                        wszDefaultNamespace,
                        System.IO.Path.GetFileNameWithoutExtension(wszInputFilePath),
                        this.CodeProvider);
                }
                catch (System.Exception ex)
                {
                    output = "// "
                             + ex.ToString()
                                 .Trim()
                                 .Replace("\r", string.Empty)
                                 .Replace("\n", System.Environment.NewLine + "// ");
                }

                var contents = System.Text.Encoding.UTF8.GetBytes(output);

                // allocate result memory and copy output string to result memory
                taskMem = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(contents.Length);
                System.Runtime.InteropServices.Marshal.Copy(contents, 0, taskMem, contents.Length);
                rgbOutputFileContents[0] = taskMem;
                taskMem = System.IntPtr.Zero;

                // set the result length and return success
                pcbOutput = (uint)contents.Length;
                return Microsoft.VisualStudio.VSConstants.S_OK;
            }
            catch
            {
                // set the result length to 0 and return failure
                pcbOutput = 0;
                return Microsoft.VisualStudio.VSConstants.VS_E_UNSUPPORTEDFORMAT;
            }
            finally
            {
                if (taskMem != System.IntPtr.Zero)
                {
                    System.Runtime.InteropServices.Marshal.FreeCoTaskMem(taskMem);
                }
            }
        }

        #endregion interface Microsoft.VisualStudio.Shell.Interop.IVsSingleFileGenerator

        #region interface Microsoft.VisualStudio.OLE.Interop.IObjectWithSite

        public void GetSite(ref System.Guid riid, out System.IntPtr ppvSite)
        {
            if (this.site == null)
            {
                System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(Microsoft.VisualStudio.VSConstants.E_NOINTERFACE);
            }

            var punk = System.IntPtr.Zero;
            int hr;

            try
            {
                // Query for the interface using the site object initially passed to the generator
                punk = System.Runtime.InteropServices.Marshal.GetIUnknownForObject(this.site);
                hr = System.Runtime.InteropServices.Marshal.QueryInterface(punk, ref riid, out ppvSite);
            }
            finally
            {
                if (punk != System.IntPtr.Zero)
                {
                    System.Runtime.InteropServices.Marshal.Release(punk);
                }
            }

            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(hr);
        }

        public void SetSite(object pUnkSite)
        {
            // Save away the site object for later use
            this.site = pUnkSite;

            // These are initialized on demand via our private CodeProvider and SiteServiceProvider properties
            this.codeDomProvider = null;
            this.serviceProvider = null;
        }

        #endregion interface Microsoft.VisualStudio.OLE.Interop.IObjectWithSite
    }
}