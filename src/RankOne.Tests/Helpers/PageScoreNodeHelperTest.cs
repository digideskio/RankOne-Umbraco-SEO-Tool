﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RankOne.Helpers;
using RankOne.Interfaces;
using RankOne.Models;
using RankOne.Serializers;
using RankOne.Tests.Mock;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace RankOne.Tests.Helpers
{
    [TestClass]
    public class PageScoreNodeHelperTest
    {
        private Mock<ITypedPublishedContentQuery> _typedPublishedContentQueryMock;
        private Mock<INodeReportRepository> _nodeReportRepositoryMock;
        private Mock<IPageScoreSerializer> _pageScoreSerializerMock;
        private Mock<IAnalyzeService> _analyzeServiceMock;
        private PageScoreNodeHelper _mockedPageScoreNodeHelper;
        private List<IPublishedContent> _nodes;

        [TestInitialize]
        public void Initialize()
        {
            _typedPublishedContentQueryMock = new Mock<ITypedPublishedContentQuery>();
            _nodeReportRepositoryMock = new Mock<INodeReportRepository>();
            _nodeReportRepositoryMock.Setup(x => x.GetById(1)).Returns(new NodeReport() { Id = 1, FocusKeyword = "focus", Report = "" });
            _nodeReportRepositoryMock.Setup(x => x.GetById(11)).Returns((NodeReport)null);
            _nodeReportRepositoryMock.Setup(x => x.GetById(12)).Returns(new NodeReport() { Id = 12, FocusKeyword = "focus", Report = "" });
            _pageScoreSerializerMock = new Mock<IPageScoreSerializer>();
            _analyzeServiceMock = new Mock<IAnalyzeService>();
            _analyzeServiceMock.Setup(x => x.CreateAnalysis(It.Is<IPublishedContent>(y => y.Id == 1), null)).Returns(new PageAnalysis() { FocusKeyword = "focus", Score = new PageScore() { OverallScore = 75 } });

            _mockedPageScoreNodeHelper = new PageScoreNodeHelper(_typedPublishedContentQueryMock.Object, _nodeReportRepositoryMock.Object, _pageScoreSerializerMock.Object,
                _analyzeServiceMock.Object);

            _nodes = new List<IPublishedContent>()
            {
                new PublishedContentMock(){
                    Id = 1,
                    Name = "node 1",
                    TemplateId = 99,
                    Children = new List<IPublishedContent> ()
                    {
                        new PublishedContentMock(){
                            Id = 11,
                            Name = "node 11",
                            TemplateId = 0,
                        },
                        new PublishedContentMock(){
                            Id = 12,
                            Name = "node 12",
                            TemplateId = 99,
                        }
                    }
                }
            };
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_OnExecuteWithNullParameterForTypedPublishedContentQuery_ThrowsException()
        {
            new PageScoreNodeHelper(null, new Mock<INodeReportRepository>().Object, new PageScoreSerializer(), new Mock<IAnalyzeService>().Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_OnExecuteWithNullParameterForNodeReportService_ThrowsException()
        {
            new PageScoreNodeHelper(new Mock<ITypedPublishedContentQuery>().Object, null, new PageScoreSerializer(), new Mock<IAnalyzeService>().Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_OnExecuteWithNullParameterForPageScoreSerializer_ThrowsException()
        {
            new PageScoreNodeHelper(new Mock<ITypedPublishedContentQuery>().Object, new Mock<INodeReportRepository>().Object, null, new Mock<IAnalyzeService>().Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_OnExecuteWithNullParameterForAnalyzeService_ThrowsException()
        {
            new PageScoreNodeHelper(new Mock<ITypedPublishedContentQuery>().Object, new Mock<INodeReportRepository>().Object, new PageScoreSerializer(), null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetPageScoresFromCache_OnExecuteWithNull_ThrowsException()
        {
            _mockedPageScoreNodeHelper.GetPageScoresFromCache(null);
        }

        [TestMethod]
        public void GetPageScoresFromCache_OnExecute_ReturnsPageScoreNode()
        {
            var result = _mockedPageScoreNodeHelper.GetPageScoresFromCache(_nodes);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(2, result.First().Children.Count());
            _nodeReportRepositoryMock.Verify(x => x.GetById(1), Times.Once);
            _nodeReportRepositoryMock.Verify(x => x.GetById(11), Times.Once);
            _nodeReportRepositoryMock.Verify(x => x.GetById(12), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UpdatePageScores_OnExecuteWithNull_ThrowsException()
        {
            _mockedPageScoreNodeHelper.UpdatePageScores(null);
        }

        [TestMethod]
        public void UpdatePageScores_OnExecute_ReturnsPageScoreNode()
        {
            var result = _mockedPageScoreNodeHelper.UpdatePageScores(_nodes);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(2, result.First().Children.Count());
            _analyzeServiceMock.Verify(x => x.CreateAnalysis(It.Is<IPublishedContent>(y => y.Id == 1), null), Times.Once);
            _analyzeServiceMock.Verify(x => x.CreateAnalysis(It.Is<IPublishedContent>(y => y.Id == 11), null), Times.Never);
            _analyzeServiceMock.Verify(x => x.CreateAnalysis(It.Is<IPublishedContent>(y => y.Id == 12), null), Times.Once);
        }
    }
}