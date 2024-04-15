using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Dazinator.Extensions.FileProviders;
using J2N.Collections.ObjectModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using NPoco.Expressions;
using Our.Umbraco.StorageProviders.AWSS3.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Web.Caching.AWS;

namespace Our.Umbraco.StorageProviders.AWSS3.Imaging;

public class AWSS3FileSystemImageCacheConfigureOptions : IConfigureNamedOptions<AWSS3StorageCacheOptions>
{
    private readonly string _name = AWSS3FileSystemOptions.MediaFileSystemName;
    private readonly IOptionsMonitor<AWSS3FileSystemOptions> _fileSystemOptions;
    private readonly AWSOptions _awsOptions;
    private readonly IAmazonS3 _s3Client;
    
    public AWSS3FileSystemImageCacheConfigureOptions(
        IOptionsMonitor<AWSS3FileSystemOptions> fileSystemOptions,
        AWSOptions awsOptions, 
        IAmazonS3 s3Client)
    {
        _fileSystemOptions = fileSystemOptions ?? throw new ArgumentNullException(nameof(fileSystemOptions));
        _awsOptions = awsOptions ?? throw new ArgumentNullException(nameof(awsOptions));
        _s3Client = s3Client;
    }
    
    public void Configure(AWSS3StorageCacheOptions options)
    {
        Configure(Options.DefaultName, options);
    }
 
    /// <summary>
    /// Return AWSS3StorageCacheOptions object with logic to check the ProfilesLocation and use that in an override
    /// as the client may want to target an alternative location
    /// </summary>
    public void Configure(string name, AWSS3StorageCacheOptions cacheOptions)
    {
        if (name != _name)
            return;

        var awss3FileSystemOptions = this._fileSystemOptions.Get(this._name);

        cacheOptions.BucketName = awss3FileSystemOptions.BucketName;
        cacheOptions.Region = getRegionName(awss3FileSystemOptions, _awsOptions);
        
        if (string.IsNullOrEmpty(cacheOptions.Region))
            cacheOptions.Endpoint = _s3Client.Config.ServiceURL;

        // if ProfilesLocation added, physical assignment of the AWS credentials must be made
        if (!string.IsNullOrEmpty(_awsOptions.ProfilesLocation))
        {
            var chain = new CredentialProfileStoreChain(_awsOptions.ProfilesLocation);
            if (chain.TryGetAWSCredentials(_awsOptions.Profile, out AWSCredentials awsCredentials))
            {
                cacheOptions.AccessKey = awsCredentials.GetCredentials().AccessKey;
                cacheOptions.AccessSecret = awsCredentials.GetCredentials().SecretKey;
            }
        }
    }
    
    private string getRegionName(AWSS3FileSystemOptions options, AWSOptions awsOptions)
    {
        // Get region -- start with fileSystemOptions; Doesn't exist? fallback to AWSOptions and then to S3Client
        string region = options.Region;
        if (string.IsNullOrEmpty(region))
        {
            if (awsOptions != null && awsOptions.Region != null)
            {
                region = awsOptions.Region.SystemName;
            }
            else if (_s3Client != null)
            {
                region = _s3Client.Config?.RegionEndpoint?.SystemName;
            }
        }

        return region;
    }
}