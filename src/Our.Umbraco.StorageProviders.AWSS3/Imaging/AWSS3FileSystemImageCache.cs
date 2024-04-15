using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.Extensions.NETCore.Setup;
using Amazon.S3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Caching.AWS;
using SixLabors.ImageSharp.Web.Resolvers;
using Our.Umbraco.StorageProviders.AWSS3.IO;
using Amazon.Util;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;

namespace Our.Umbraco.StorageProviders.AWSS3.Imaging
{
    /// <summary>
    /// Implements an S3 based cache storing files in a <c>cache</c> subfolder.
    /// </summary>
    public class AWSS3FileSystemImageCache : IImageCache
    {
        private const string _cachePath = "cache/";
        private readonly string _name;
        private AWSS3StorageCache baseCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="AWSS3FileSystemImageCache" /> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public AWSS3FileSystemImageCache(IOptionsMonitor<AWSS3StorageCacheOptions> options)
            : this(AWSS3FileSystemOptions.MediaFileSystemName, options)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="AWSS3FileSystemImageCache" />.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="options">The options.</param>
        /// <exception cref="System.ArgumentNullException">options
        /// or
        /// name</exception>
        protected AWSS3FileSystemImageCache(string name, IOptionsMonitor<AWSS3StorageCacheOptions> options)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
            
            baseCache = new AWSS3StorageCache(Options.Create(options.Get(_name)));

            options.OnChange(OptionsOnChange);
        }

        /// <inheritdoc/>
        public async Task<IImageCacheResolver> GetAsync(string key)
        {
            string cacheAndKey = Path.Combine(_cachePath, key);

            return await baseCache.GetAsync(cacheAndKey);
        }

        /// <inheritdoc/>
        public Task SetAsync(string key, Stream stream, ImageCacheMetadata metadata)
        {
            string cacheAndKey = Path.Combine(_cachePath, key);
            return baseCache.SetAsync(cacheAndKey, stream, metadata);
        }

        private void OptionsOnChange(AWSS3StorageCacheOptions options, string name)
        {
            if (name != _name) return;
            
            baseCache = new AWSS3StorageCache(Options.Create(options));
        }

        


        
    }
}
