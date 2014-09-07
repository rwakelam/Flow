using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SynchronisationLibrary;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

namespace SynchronisationTest
{
    [TestClass]
    public class PusherTests
    {
        [TestMethod]
        public void TestPushDirectoryWhenSourcePathIsNull()
        {
            var fileSystem = new System.IO.Abstractions.TestingHelpers.MockFileSystem();
            fileSystem.AddDirectory(@"C:\Target");
            try
            {
                Pusher.PushDirectory(fileSystem, null, @"C:\Target");
                Assert.Fail("Exception not raised.");
            }
            catch (ArgumentNullException)
            {
            }
            catch
            {
                Assert.Fail("Wrong exception raised.");
            }
        }

        [TestMethod]
        public void TestPushDirectoryWhenTargetPathIsNull()
        {
            var fileSystem = new System.IO.Abstractions.TestingHelpers.MockFileSystem();
            fileSystem.AddDirectory(@"C:\Source");
            try
            {
                Pusher.PushDirectory(fileSystem, @"C:\Source", null);
                Assert.Fail("Exception not raised.");
            }
            catch (ArgumentNullException)
            {
            }
            catch
            {
                Assert.Fail("Wrong exception raised.");
            }
        }

        [TestMethod]
        public void TestPushDirectoryWhenSourceIsNotFound()
        {
            var fileSystem = new System.IO.Abstractions.TestingHelpers.MockFileSystem();
            fileSystem.AddDirectory(@"C:\Target");
            try
            {
                Pusher.PushDirectory(fileSystem, @"C:\Source", @"C:\Target");
                Assert.Fail("Exception not raised.");
            }
            catch (SyncDirectoryNotFoundException)
            {
            }
            catch
            {
                Assert.Fail("Wrong exception raised.");
            }
        }

        [TestMethod]
        public void TestPushFileWhenSourceIsNull()
        {
            var fileSystem = new System.IO.Abstractions.TestingHelpers.MockFileSystem();
            fileSystem.AddFile(@"C:\Target\File.txt", new MockFileData("Data")); 
            try
            {
                Pusher.PushFile(fileSystem, null, @"C:\Target\File.txt");
                Assert.Fail("Exception not raised.");
            }
            catch (ArgumentNullException)
            {
            }
            catch
            {
                Assert.Fail("Wrong exception raised.");
            }
        }

        [TestMethod]
        public void TestPushFileWhenTargetIsNull()
        {
            var fileSystem = new System.IO.Abstractions.TestingHelpers.MockFileSystem();
            fileSystem.AddFile(@"C:\Source\File.txt", new MockFileData("Data"));
            try
            {
                Pusher.PushFile(fileSystem, @"C:\Source\File.txt", null);
                Assert.Fail("Exception not raised.");
            }
            catch (ArgumentNullException)
            {
            }
            catch
            {
                Assert.Fail("Wrong exception raised.");
            }
        }

        [TestMethod]
        public void TestPushFileWhenSourceIsNotFound()
        {            
            var fileSystem = new System.IO.Abstractions.TestingHelpers.MockFileSystem();
            try
            {
                Pusher.PushFile(fileSystem, @"C:\Source\File.txt", @"C:\Target\File.txt");
                Assert.Fail("Exception not raised.");
            }
            catch (SyncFileNotFoundException)
            {
            }
            catch
            {
                Assert.Fail("Wrong exception raised.");
            }
        }

        [TestMethod]
        public void TestPushFileWhenBytesNotEqual()
        {
            // Prepare the source and target directories and files.
            var fileSystem = new System.IO.Abstractions.TestingHelpers.MockFileSystem();
            fileSystem.AddFile(@"C:\Source\File.txt", new MockFileData("Data2"));
            fileSystem.AddFile(@"C:\Target\File.txt", new MockFileData("Data"));
            fileSystem.File.SetAttributes(@"C:\Source\File.txt", FileAttributes.Hidden);
            fileSystem.File.SetAttributes(@"C:\Target\File.txt", FileAttributes.Normal);
            Pusher.PushEntryResult result = Pusher.PushFile(fileSystem,
                @"C:\Source\File.txt", @"C:\Target\File.txt");
            Assert.AreEqual("Data2", fileSystem.File.ReadAllText(@"C:\Target\File.txt"));
            Assert.AreEqual(FileAttributes.Hidden, fileSystem.File.GetAttributes(@"C:\Target\File.txt"));
        }

        [TestMethod]
        public void TestPushFileWhenAttributesNotEqual()
        {
            // Prepare the source and target directories and files.
            var fileSystem = new System.IO.Abstractions.TestingHelpers.MockFileSystem();
            fileSystem.AddFile(@"C:\Source\File.txt", new MockFileData("Data"));
            fileSystem.AddFile(@"C:\Target\File.txt", new MockFileData("Data"));
            fileSystem.File.SetAttributes(@"C:\Source\File.txt", FileAttributes.Hidden);
            fileSystem.File.SetAttributes(@"C:\Target\File.txt", FileAttributes.Normal);
            Pusher.PushEntryResult result = Pusher.PushFile(fileSystem,
                @"C:\Source\File.txt", @"C:\Target\File.txt");
            Assert.AreEqual(FileAttributes.Hidden, 
                fileSystem.File.GetAttributes(@"C:\Target\File.txt"));
        }

        [TestMethod]
        public void TestPushNewFile()
        {
            // Prepare the source and target directories and files.
            var fileSystem = new System.IO.Abstractions.TestingHelpers.MockFileSystem();
            fileSystem.AddFile(@"C:\Source\File.txt", new MockFileData("Data"));
            fileSystem.File.SetAttributes(@"C:\Source\File.txt", FileAttributes.Archive);
            Pusher.PushEntryResult result = Pusher.PushFile(fileSystem, @"C:\Source\File.txt",
                @"C:\Target\File.txt");
            Assert.AreEqual("Data", fileSystem.File.ReadAllText(@"C:\Target\File.txt"));
            Assert.AreEqual(FileAttributes.Archive, fileSystem.File.GetAttributes(@"C:\Target\File.txt"));
        }

        [TestMethod]
        public void TestPushDirectoryByPattern()
        {
            // Prepare the source and target directories and files.
            var fileSystem = new System.IO.Abstractions.TestingHelpers.MockFileSystem();
            fileSystem.AddFile(@"C:\Source\File.txt", new MockFileData("Data"));
            fileSystem.AddFile(@"C:\Source\File.doc", new MockFileData("Data"));

            // Run the test.
            Pusher.PushDirectoryResult result = Pusher.PushDirectory(fileSystem,
                @"C:\Source", @"C:\Target", "*.txt");

            // Check the results
            Assert.AreEqual("Data", fileSystem.File.ReadAllText(@"C:\Target\File.txt"));
            Assert.IsFalse(fileSystem.File.Exists(@"C:\Target\File.doc"));
        }

        [TestMethod]
        public void TestPushDirectoryByAttributes()
        {
            // Prepare the source and target directories and files.
            var fileSystem = new System.IO.Abstractions.TestingHelpers.MockFileSystem();
            fileSystem.AddFile(@"C:\Source\File1.txt", new MockFileData("Data"));
            fileSystem.AddFile(@"C:\Source\File2.txt", new MockFileData("Data"));
            fileSystem.File.SetAttributes(@"C:\Source\File1.txt", FileAttributes.Hidden);
            fileSystem.File.SetAttributes(@"C:\Source\File2.txt", FileAttributes.Archive);

            // Run the test.
            Pusher.PushDirectoryResult result = Pusher.PushDirectory(fileSystem, 
                @"C:\Source", @"C:\Target", "*.*", FileAttributes.Hidden);

            // Check the results
            Assert.AreEqual("Data", fileSystem.File.ReadAllText(@"C:\Target\File1.txt"));
            Assert.IsFalse(fileSystem.File.Exists(@"C:\Target\File2.txt"));
        }

        [TestMethod]
        public void TestPushDirectoryByPatternAndAttributes()
        {
            // Prepare the source and target directories and files.
            var fileSystem = new System.IO.Abstractions.TestingHelpers.MockFileSystem();
            fileSystem.AddFile(@"C:\Source\File1.txt", new MockFileData("Data"));
            fileSystem.AddFile(@"C:\Source\File2.txt", new MockFileData("Data"));
            fileSystem.AddFile(@"C:\Source\File3.doc", new MockFileData("Data"));
            fileSystem.File.SetAttributes(@"C:\Source\File1.txt", FileAttributes.Hidden);
            fileSystem.File.SetAttributes(@"C:\Source\File2.txt", FileAttributes.Archive);
            fileSystem.File.SetAttributes(@"C:\Source\File3.doc", FileAttributes.Archive);

            // Run the test.
            Pusher.PushDirectoryResult result = Pusher.PushDirectory(fileSystem, 
                @"C:\Source", @"C:\Target", "*.txt", FileAttributes.Archive);

            // Check the results
            Assert.IsFalse(fileSystem.File.Exists(@"C:\Target\File1.txt"));
            Assert.AreEqual("Data", fileSystem.File.ReadAllText(@"C:\Target\File2.txt"));
            Assert.IsFalse(fileSystem.File.Exists(@"C:\Target\File3.doc"));
        }
        
    }
}
