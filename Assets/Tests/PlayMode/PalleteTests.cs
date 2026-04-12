using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

/// <summary>
/// Tests for PalleteExpand component behavior.
/// Covers section expansion and the indexOfExpandedSection property.
/// </summary>
[TestFixture]
public class PalleteExpandTests
{
    private PalleteExpand palleteExpand;
    private LayoutElement[] sectionElements;

    [SetUp]
    public void SetUp()
    {
        GameObject go = new GameObject("PalleteExpand");
        go.AddComponent<RectTransform>();

        // Create 3 child sections with LayoutElements and Buttons
        sectionElements = new LayoutElement[3];
        for (int i = 0; i < 3; i++)
        {
            GameObject child = new GameObject("Section" + i);
            child.transform.SetParent(go.transform);
            sectionElements[i] = child.AddComponent<LayoutElement>();
            child.AddComponent<Button>();
        }

        palleteExpand = go.AddComponent<PalleteExpand>();
    }

    [TearDown]
    public void TearDown()
    {
        if (palleteExpand != null && palleteExpand.gameObject != null)
        {
            Object.DestroyImmediate(palleteExpand.gameObject);
        }
    }

    [Test]
    public void PalleteExpand_DefaultExpandedIndex_IsOne()
    {
        Assert.AreEqual(1, palleteExpand.indexOfExpandedSection);
    }

    [Test]
    public void PalleteExpand_IndexCanBeChanged()
    {
        palleteExpand.indexOfExpandedSection = 2;
        Assert.AreEqual(2, palleteExpand.indexOfExpandedSection);
    }

    [Test]
    public void PalleteExpand_IndexCanBeSetToZero()
    {
        palleteExpand.indexOfExpandedSection = 0;
        Assert.AreEqual(0, palleteExpand.indexOfExpandedSection);
    }
}

/// <summary>
/// Tests for PalleteScript component behavior.
/// </summary>
[TestFixture]
public class PalleteScriptTests
{
    [Test]
    public void PalleteScript_CanBeAddedToGameObject()
    {
        GameObject go = new GameObject("Pallete");
        go.AddComponent<Animator>();
        PalleteScript ps = go.AddComponent<PalleteScript>();

        Assert.IsNotNull(ps);

        Object.DestroyImmediate(go);
    }
}

/// <summary>
/// Tests for PalleteSection component behavior.
/// </summary>
[TestFixture]
public class PalleteSectionTests
{
    [Test]
    public void PalleteSection_CanBeAddedToGameObject()
    {
        GameObject go = new GameObject("Section");
        go.AddComponent<RectTransform>();
        go.AddComponent<VerticalLayoutGroup>();
        PalleteSection ps = go.AddComponent<PalleteSection>();

        Assert.IsNotNull(ps);

        Object.DestroyImmediate(go);
    }
}
