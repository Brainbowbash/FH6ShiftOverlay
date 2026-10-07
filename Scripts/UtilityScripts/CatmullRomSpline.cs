// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: 2026 Brainbowbash
using Godot;
using System;
using System.Collections.Generic;
namespace Utility;
public partial class CatmullRomSpline : RefCounted
{
	// apologies for the spaghetti
	struct TValuesPair
	{
		public float T;
		public float[] Values;
	}

	public float minBounds { get { return _minBounds; } }
	public float maxBounds { get { return _maxBounds; } }
	private float _minBounds;
	private float _maxBounds;

	private int vectorLength;

	private List<TValuesPair> splinePairs;

	private int ReturnArrayIndex(List<TValuesPair> pairs, float t)
	{
		if (pairs.Count == 0) return -1;

		int low = 0;
		int high = pairs.Count - 1;

		while (low <= high)
		{
			int mid = low + (high - low) / 2;
			if (pairs[mid].T == t) return mid;
			if (pairs[mid].T < t) low = mid + 1;
			else high = mid - 1;
		}
		return high;
	}

	private void Lerp(Span<float> v1, Span<float> v2, float weight1, float weight2, Span<float> output)
	{
		for (int i = 0; i < output.Length; i++)
			output[i] = v1[i] * weight1 + v2[i] * weight2;
	}

	public float[] Sample(float t)
	{
		if (splinePairs.Count < 4)
		{
			return new float[vectorLength];
		}

		int index = Math.Clamp(ReturnArrayIndex(splinePairs, t), 1, splinePairs.Count - 3);

		TValuesPair pair0 = splinePairs[index - 1];
		TValuesPair pair1 = splinePairs[index];
		TValuesPair pair2 = splinePairs[index + 1];
		TValuesPair pair3 = splinePairs[index + 2];

		Span<float> A1 = stackalloc float[vectorLength];
		Span<float> A2 = stackalloc float[vectorLength];
		Span<float> A3 = stackalloc float[vectorLength];
		Span<float> B1 = stackalloc float[vectorLength];
		Span<float> B2 = stackalloc float[vectorLength];
		Span<float> result = stackalloc float[vectorLength];

		Lerp(pair0.Values, pair1.Values, (pair1.T - t) / (pair1.T - pair0.T), (t - pair0.T) / (pair1.T - pair0.T), A1);
		Lerp(pair1.Values, pair2.Values, (pair2.T - t) / (pair2.T - pair1.T), (t - pair1.T) / (pair2.T - pair1.T), A2);
		Lerp(pair2.Values, pair3.Values, (pair3.T - t) / (pair3.T - pair2.T), (t - pair2.T) / (pair3.T - pair2.T), A3);

		Lerp(A1, A2, (pair2.T - t) / (pair2.T - pair0.T), (t - pair0.T) / (pair2.T - pair0.T), B1);
		Lerp(A2, A3, (pair3.T - t) / (pair3.T - pair1.T), (t - pair1.T) / (pair3.T - pair1.T), B2);

		Lerp(B1, B2, (pair2.T - t) / (pair2.T - pair1.T), (t - pair1.T) / (pair2.T - pair1.T), result);

		float[] output = new float[vectorLength];
		result.CopyTo(output);
		return output;
	}

	public bool InsertPoint(float t, float[] values)
	{
		t = MathF.Round(t / 100f) * 100f;
		if (splinePairs == null)
		{
			splinePairs = new List<TValuesPair>();
			splinePairs.Add(new TValuesPair { T = t, Values = values });
			UpdateBounds();
			return true;
		}

		int floorIndex = ReturnArrayIndex(splinePairs, t);

		int insertIndex = floorIndex + 1;

		if (floorIndex >= 0 && floorIndex < splinePairs.Count && Mathf.IsEqualApprox(splinePairs[floorIndex].T, t))
		{
			bool greaterThanSpline = false;

			for (int i = 0; i < values.Length; i++)
			{
				greaterThanSpline |= values[i] > splinePairs[floorIndex].Values[i];
				values[i] = MathF.Max(values[i], splinePairs[floorIndex].Values[i]);
			}

			if (greaterThanSpline)
			{
				splinePairs[floorIndex] = new TValuesPair { T = t, Values = values };
			}
			else
			{
				return false;
			}
		}
		else
		{
			splinePairs.Insert(insertIndex, new TValuesPair { T = t, Values = values });
		}

		UpdateBounds();
		return true;
	}

	public void InsertPoint(float t, float value)
	{
		InsertPoint(t, new float[] { value });
	}

	private void UpdateBounds()
	{
		if (splinePairs.Count >= 4)
		{
			_minBounds = splinePairs[1].T;
			_maxBounds = splinePairs[splinePairs.Count - 2].T;
		}
		else
		{
			_minBounds = splinePairs.Count > 0 ? splinePairs[0].T : 0;
			_maxBounds = splinePairs.Count > 0 ? splinePairs[splinePairs.Count - 1].T : 0;
		}
	}

	public CatmullRomSpline(float[][] values, float[] t)
	{
		if (values.Length != t.Length)
		{
			splinePairs = new List<TValuesPair>();
			return;
		}
		splinePairs = new List<TValuesPair>(values.Length);
		for (int i = 0; i < values.Length; i++)
		{
			splinePairs.Add(new TValuesPair { T = t[i], Values = values[i] });
		}
		UpdateBounds();

		vectorLength = values[0].Length;
	}

	public CatmullRomSpline(float[] values, float[] t)
	{
		if (values.Length != t.Length)
		{
			splinePairs = new List<TValuesPair>();
			return;
		}
		splinePairs = new List<TValuesPair>(values.Length);
		for (int i = 0; i < values.Length; i++)
		{
			splinePairs.Add(new TValuesPair { T = t[i], Values = new float[] { values[i] } });
		}
		UpdateBounds();
		vectorLength = 1;
	}

	public void Clear()
	{
		splinePairs.Clear();
		_minBounds = 0f;
		_maxBounds = 0f;
	}
}
