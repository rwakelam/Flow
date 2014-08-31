using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SynchronisationLibrary;
using WrapperLibrary;

namespace SynchronisationTest
{
    [TestClass]
    public class PusherTests
    {
        [TestMethod]
        public void TestPushDirectoryWhenSourceIsNull()
        {
            IDirectory targetDirectory = Common.CreateDirectory(Common.TargetDirectoryPath);
            try
            {
                Pusher.Push(null, targetDirectory);
                Assert.Fail("Required exception not raised.");
            }
            catch (ArgumentNullException ex)
            {
            }
        }
        
        [TestMethod]
        public void TestPushDirectoryWhenTargetIsNull()
        {
            IDirectory sourceDirectory = Common.CreateDirectory(Common.SourceDirectoryPath);
            try
            {
                Pusher.Push(sourceDirectory, null);
                Assert.Fail("Required exception not raised.");
            }
            catch (ArgumentNullException ex)
            {
            }
        }

        [TestMethod]
        public void TestPushDirectoryWhenSourceIsNotFound()
        {
            IDirectory sourceDirectory = new DirectoryWrapper(Common.SourceDirectoryPath);
            IDirectory targetDirectory = Common.CreateDirectory(Common.TargetDirectoryPath);
            if (sourceDirectory.Exists())
            {
                sourceDirectory.Delete();
            }
            try
            {
                Pusher.Push(sourceDirectory, targetDirectory);
                Assert.Fail("Required exception not raised.");
            }
            catch (SyncDirectoryNotFoundException ex)
            {
            }
        }

        [TestMethod]
        public void TestPushFileWhenSourceIsNull()
        {
            IFile targetFile = Common.CreateFile("File.txt", Common.TargetDirectoryPath, new byte[]{0});
            try
            {
                Pusher.Push(null, targetFile);
                Assert.Fail("Required exception not raised.");
            }
            catch (ArgumentNullException ex)
            {
            }
        }

        [TestMethod]
        public void TestPushFileWhenTargetIsNull()
        {
            IFile sourceFile = Common.CreateFile("File.txt", Common.SourceDirectoryPath, new byte[] { 0 });
            try
            {
                Pusher.Push(null, sourceFile);
                Assert.Fail("Required exception not raised.");
            }
            catch (ArgumentNullException ex)
            {
            }
        }

        [TestMethod]
        public void TestPushFileWhenSourceIsNotFound()
        {
            IFile sourceFile = new FileWrapper(Path.Combine(Common.SourceDirectoryPath, "File.txt"));
            IFile targetFile = Common.CreateFile("File.txt", Common.TargetDirectoryPath, new byte[]{0});
            if (sourceFile.Exists())
            {
                sourceFile.Delete();
            }
            try
            {
                Pusher.Push(sourceFile, targetFile);
                Assert.Fail("Required exception not raised.");
            }
            catch (SyncFileNotFoundException ex)
            {
            }        
        }

        [TestMethod]
        public void TestPushFileWhenBytesNotEqual()
        {
            // Prepare the source and target directories and files.
            IDirectory sourceDirectory = Common.CreateDirectory(Common.SourceDirectoryPath);
            IDirectory targetDirectory = Common.CreateDirectory(Common.TargetDirectoryPath);
            IFile sourceFile = Common.CreateFile("File.txt", Common.SourceDirectoryPath, new byte[] { 65 });
            IFile targetFile = Common.CreateFile("File.txt", Common.TargetDirectoryPath, new byte[] { 66 }); 
            Pusher.PushEntryResult result = Pusher.Push(sourceFile, targetFile);
            Assert.AreEqual(Pusher.PushEntryResult.Updated, result);
        }

        [TestMethod]
        public void TestPushFileWhenAttributesNotEqual()
        {
            // Prepare the source and target directories and files.
            IDirectory sourceDirectory = Common.CreateDirectory(Common.SourceDirectoryPath);
            IDirectory targetDirectory = Common.CreateDirectory(Common.TargetDirectoryPath);
            IFile sourceFile = Common.CreateFile("File.txt", Common.SourceDirectoryPath, new byte[] { 65 },
                FileAttributesWrapper.Normal);
            IFile targetFile = Common.CreateFile("File.txt", Common.TargetDirectoryPath, new byte[] { 66 },
                FileAttributesWrapper.Archive);
            Pusher.PushEntryResult result = Pusher.Push(sourceFile, targetFile);
            Assert.AreEqual(Pusher.PushEntryResult.Updated, result);
        }

        [TestMethod]
        public void TestPushNewFile()
        {
            // Prepare the source and target directories and files.
            IDirectory sourceDirectory = Common.CreateDirectory(Common.SourceDirectoryPath);
            IDirectory targetDirectory = Common.CreateDirectory(Common.TargetDirectoryPath);
            IFile sourceFile = Common.CreateFile("File.txt", Common.SourceDirectoryPath, new byte[] { 65 });
            IFile targetFile = new FileWrapper(Path.Combine(Common.TargetDirectoryPath, "File.txt"));
            if (targetFile.Exists()) 
            {
                targetFile.Delete();
            }
            Pusher.PushEntryResult result = Pusher.Push(sourceFile, targetFile);
            Assert.AreEqual(Pusher.PushEntryResult.Created, result);
        }

        [TestMethod]
        public void TestPushFileWhenTargetMatchesSource()
        {
            // Prepare the source and target directories and files.
            IDirectory sourceDirectory = Common.CreateDirectory(Common.SourceDirectoryPath);
            IDirectory targetDirectory = Common.CreateDirectory(Common.TargetDirectoryPath);
            IFile sourceFile = Common.CreateFile("File.txt", Common.SourceDirectoryPath, new byte[] { 65 });
            IFile targetFile = Common.CreateFile("File.txt", Common.TargetDirectoryPath, new byte[] { 65 });
            Pusher.PushEntryResult result = Pusher.Push(sourceFile, targetFile);
            Assert.AreEqual(Pusher.PushEntryResult.Verified, result);
        }

        [TestMethod]
        public void TestPushDirectoryByPattern()
        {
            // Prepare the source and target directories and files.
            IDirectory sourceDirectory = Common.CreateDirectory(Common.SourceDirectoryPath);
            IDirectory targetDirectory = Common.CreateDirectory(Common.TargetDirectoryPath);
            IFile soughtFile = Common.CreateFile("File.txt", Common.SourceDirectoryPath, new byte[] { 65 });
            IFile unsoughtFile = Common.CreateFile("File.doc", Common.SourceDirectoryPath, new byte[] { 65 });

            // Run the test.
            Pusher.PushDirectoryResult result = Pusher.Push(sourceDirectory, targetDirectory, "*.txt");

            // Check the results
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.CreatedFiles);
            Assert.AreEqual(1, result.CreatedFiles.Count);
            Assert.AreEqual(soughtFile.Name, result.CreatedFiles[0]);
            Assert.IsNotNull(result.FailedEntries);
            Assert.AreEqual(0, result.FailedEntries.Count);
            Assert.IsNotNull(result.UpdatedFiles);
            Assert.AreEqual(0, result.UpdatedFiles.Count);
            Assert.IsNotNull(result.VerifiedFiles);
            Assert.AreEqual(0, result.VerifiedFiles.Count);
            Assert.IsNotNull(result.DirectoryResults);
            Assert.AreEqual(0, result.DirectoryResults.Count);
        }
        
        [TestMethod]
        public void TestPushDirectoryByAttributes()
        {
            // Prepare the source and target directories and files.
            IDirectory sourceDirectory = Common.CreateDirectory(Common.SourceDirectoryPath);
            IDirectory targetDirectory = Common.CreateDirectory(Common.TargetDirectoryPath);
            IFile soughtFile = Common.CreateFile("File.txt", Common.SourceDirectoryPath, new byte[] { 65 }, 
                FileAttributesWrapper.Normal);
            IFile unsoughtFile = Common.CreateFile("File2.txt", Common.SourceDirectoryPath, new byte[] { 65 }, 
                FileAttributesWrapper.Hidden);

            // Run the test.
            Pusher.PushDirectoryResult result = Pusher.Push(sourceDirectory, targetDirectory, null, 
                FileAttributesWrapper.Normal);

            // Check the results
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.CreatedFiles);
            Assert.AreEqual(1, result.CreatedFiles.Count);
            Assert.AreEqual(soughtFile.Name, result.CreatedFiles[0]);
            Assert.IsNotNull(result.FailedEntries);
            Assert.AreEqual(0, result.FailedEntries.Count);
            Assert.IsNotNull(result.UpdatedFiles);
            Assert.AreEqual(0, result.UpdatedFiles.Count);
            Assert.IsNotNull(result.VerifiedFiles);
            Assert.AreEqual(0, result.VerifiedFiles.Count);
            Assert.IsNotNull(result.DirectoryResults);
            Assert.AreEqual(0, result.DirectoryResults.Count);
        }

        [TestMethod]
        public void TestPushDirectoryByPatternAndAttributes()// More like SyncDirectory? UpdateDirectory
        {
            // Prepare the source and target directories and files.
            IDirectory sourceDirectory = Common.CreateDirectory(Common.SourceDirectoryPath);
            IDirectory targetDirectory = Common.CreateDirectory(Common.TargetDirectoryPath);
            IFile soughtFile = Common.CreateFile("File.txt", Common.SourceDirectoryPath, new byte[] { 65 }, FileAttributesWrapper.Archive);
            IFile unsoughtFile1 = Common.CreateFile("File.doc", Common.SourceDirectoryPath, new byte[] { 65 }, FileAttributesWrapper.Archive);
            IFile unsoughtFile2 = Common.CreateFile("File2.txt", Common.SourceDirectoryPath, new byte[] { 65 }, FileAttributesWrapper.Normal);

            // Run the test.
            Pusher.PushDirectoryResult result = Pusher.Push(sourceDirectory, targetDirectory, "*.txt", FileAttributesWrapper.Archive);

            // Check the results
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.CreatedFiles);
            Assert.AreEqual(1, result.CreatedFiles.Count);
            Assert.AreEqual(soughtFile.Name, result.CreatedFiles[0]);
            Assert.IsNotNull(result.FailedEntries);
            Assert.AreEqual(0, result.FailedEntries.Count);
            Assert.IsNotNull(result.UpdatedFiles);
            Assert.AreEqual(0, result.UpdatedFiles.Count);
            Assert.IsNotNull(result.VerifiedFiles);
            Assert.AreEqual(0, result.VerifiedFiles.Count);
            Assert.IsNotNull(result.DirectoryResults);
            Assert.AreEqual(0, result.DirectoryResults.Count);
        }
        
    }
}
