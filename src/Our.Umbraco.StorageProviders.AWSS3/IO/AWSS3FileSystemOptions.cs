using Amazon.S3;
using System.ComponentModel.DataAnnotations;

namespace Our.Umbraco.StorageProviders.AWSS3.IO
{
    public class AWSS3FileSystemOptions
    {
        /// <summary>
        /// The media filesystem name.
        /// </summary>
        public const string MediaFileSystemName = "Media";

        /// <summary>
        /// The prefix for the media files name string.
        /// </summary>
        public const string MediaBucketPrefix = "media";

        /// <summary>
        /// The region for the bucket
        /// </summary>
        public string Region { get; set; } = null!;


        /// <summary>
        /// The buckets name string.
        /// </summary>
        [Required]
        public string BucketName { get; set; } = null!;
        public string BucketPrefix { get; set; } = MediaBucketPrefix;

        /// <summary>
        /// The virtual path.
        /// </summary>
        [Required]
        public string VirtualPath { get; set; } = null!;



        public string BucketHostName { get; set; } = null!;

        public S3CannedACL CannedACL { get; set; }

        public ServerSideEncryptionMethod ServerSideEncryptionMethod { get; set; }
    }
}
