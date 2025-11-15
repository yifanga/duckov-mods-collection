
using System.Collections.Generic;
using UnityEngine;

namespace DuckovBetterRealDog
{
    public static class LetterPatterns
    {
        public static Dictionary<char, List<Vector2>> Data = new Dictionary<char, List<Vector2>>();

        public static void Init()
        {

            // A字母模式
            Data['A'] = new List<Vector2>
            {
                // 从下到上构建
                new(-1.0f, 0f),new(2.0f, 0f),
                new(-1.0f, 1f),new(0.0f, 1f),new(1.0f, 1f),new(2.0f, 1f),
                new(-1.0f, 2f),new(2.0f, 2f),
                new(-1.0f, 3f),new(2.0f, 3f),
                new(0.0f, 4f),new(1.0f, 4f),

            };

            // B字母模式
            Data['B'] = new List<Vector2>
            {
                // 从下到上构建
                new(-1.0f, 0f),new(0.0f, 0f),new(1.0f, 0f),
                new(-1.0f, 1f),new(2.0f, 1f),
                new(-1.0f, 2f),new(0.0f, 2f),new(1.0f, 2f),
                new(-1.0f, 3f),new(2.0f, 3f),
                new(-1.0f, 4f),new(0.0f, 4f),new(1.0f, 4f),

            };

            // C字母模式
            Data['C'] = new List<Vector2>
            {
                // 从下到上构建
                new(0.0f, 0f),new(1.0f, 0f),
                new(-1.0f, 1f),new(2.0f, 1f),
                new(-1.0f, 2f),
                new(-1.0f, 3f),new(2.0f, 3f),
                new(0.0f, 4f),new(1.0f, 4f),

            };

            // D字母模式
            Data['D'] = new List<Vector2>
            {
                // 从下到上构建
                new(-1.0f, 0f),new(0.0f, 0f),new(1.0f, 0f),
                new(-1.0f, 1f),new(2.0f, 1f),
                new(-1.0f, 2f),new(2.0f, 2f),
                new(-1.0f, 3f),new(2.0f, 3f),
                new(-1.0f, 4f),new(0.0f, 4f),new(1.0f, 4f),

            };

            // E字母模式
            Data['E'] = new List<Vector2>
            {
                // 从下到上构建
                new(0.0f, 0f),new(1.0f, 0f),new(2.0f, 0f),
                new(-1.0f, 1f),
                new(-1.0f, 2f),new(0.0f, 2f),new(1.0f, 2f),
                new(-1.0f, 3f),
                new(0.0f, 4f),new(1.0f, 4f),new(2.0f, 4f),

            };

            // F字母模式
            Data['F'] = new List<Vector2>
            {
                // 从下到上构建
                new(-1.0f, 0f),
                new(-1.0f, 1f),
                new(-1.0f, 2f),new(0.0f, 2f),new(1.0f, 2f),
                new(-1.0f, 3f),
                new(0.0f, 4f),new(1.0f, 4f),new(2.0f, 4f),

            };

            // G字母模式
            Data['G'] = new List<Vector2>
            {
                // 从下到上构建
                new(0.0f, 0f),new(1.0f, 0f),
                new(-1.0f, 1f),new(2.0f, 1f),
                new(-1.0f, 2f),new(1.0f, 2f),new(2.0f, 2f),
                new(-1.0f, 3f),
                new(0.0f, 4f),new(1.0f, 4f),new(2.0f, 4f),

            };

            // H字母模式
            Data['H'] = new List<Vector2>
            {
                // 从下到上构建
                new(-1.0f, 0f),new(2.0f, 0f),
                new(-1.0f, 1f),new(2.0f, 1f),
                new(-1.0f, 2f),new(0.0f, 2f),new(1.0f, 2f),new(2.0f, 2f),
                new(-1.0f, 3f),new(2.0f, 3f),
                new(-1.0f, 4f),new(2.0f, 4f),

            };

            // I字母模式
            Data['I'] = new List<Vector2>
            {
                // 从下到上构建
                new(-0.5f, 0f),new(0.5f, 0f),new(1.5f, 0f),
                new(0.5f, 1f),
                new(0.5f, 2f),
                new(0.5f, 3f),
                new(-0.5f, 4f),new(0.5f, 4f),new(1.5f, 4f),

            };

            // J字母模式
            Data['J'] = new List<Vector2>
            {
                // 从下到上构建
                new(0.5f, 0f),new(1.5f, 0f),
                new(-0.5f, 1f),new(1.5f, 1f),
                new(1.5f, 2f),
                new(1.5f, 3f),
                new(-0.5f, 4f),new(0.5f, 4f),new(1.5f, 4f),

            };

            // K字母模式
            Data['K'] = new List<Vector2>
            {
                // 从下到上构建
                new(-1.0f, 0f),new(2.0f, 0f),
                new(-1.0f, 1f),new(1.0f, 1f),
                new(-1.0f, 2f),new(0.0f, 2f),
                new(-1.0f, 3f),new(1.0f, 3f),
                new(-1.0f, 4f),new(2.0f, 4f),

            };

            // L字母模式
            Data['L'] = new List<Vector2>
            {
                // 从下到上构建
                new(-0.5f, 0f),new(0.5f, 0f),new(1.5f, 0f),
                new(-0.5f, 1f),
                new(-0.5f, 2f),
                new(-0.5f, 3f),
                new(-0.5f, 4f),

            };

            // M字母模式
            Data['M'] = new List<Vector2>
            {
                // 从下到上构建
                new(-1.5f, 0f),new(2.5f, 0f),
                new(-1.5f, 1f),new(2.5f, 1f),
                new(-1.5f, 2f),new(0.5f, 2f),new(2.5f, 2f),
                new(-1.5f, 3f),new(0.5f, 3f),new(2.5f, 3f),
                new(-0.5f, 4f),new(1.5f, 4f),

            };

            // N字母模式
            Data['N'] = new List<Vector2>
            {
                // 从下到上构建
                new(-1.0f, 0f),new(2.0f, 0f),
                new(-1.0f, 1f),new(1.0f, 1f),new(2.0f, 1f),
                new(-1.0f, 2f),new(0.0f, 2f),new(2.0f, 2f),
                new(-1.0f, 3f),new(2.0f, 3f),
                new(-1.0f, 4f),new(2.0f, 4f),

            };

            // O字母模式
            Data['O'] = new List<Vector2>
            {
                // 从下到上构建
                new(0.0f, 0f),new(1.0f, 0f),
                new(-1.0f, 1f),new(2.0f, 1f),
                new(-1.0f, 2f),new(2.0f, 2f),
                new(-1.0f, 3f),new(2.0f, 3f),
                new(0.0f, 4f),new(1.0f, 4f),

            };

            // P字母模式
            Data['P'] = new List<Vector2>
            {
                // 从下到上构建
                new(-1.0f, 0f),
                new(-1.0f, 1f),
                new(-1.0f, 2f),new(0.0f, 2f),new(1.0f, 2f),
                new(-1.0f, 3f),new(2.0f, 3f),
                new(0.0f, 4f),new(1.0f, 4f),

            };

            // Q字母模式
            Data['Q'] = new List<Vector2>
            {
                // 从下到上构建
                new(-0.5f, 0f),new(0.5f, 0f),new(2.5f, 0f),
                new(-1.5f, 1f),new(1.5f, 1f),
                new(-1.5f, 2f),new(1.5f, 2f),
                new(-1.5f, 3f),new(1.5f, 3f),
                new(-0.5f, 4f),new(0.5f, 4f),

            };

            // R字母模式
            Data['R'] = new List<Vector2>
            {
                // 从下到上构建
                new(-1.0f, 0f),new(2.0f, 0f),
                new(-1.0f, 1f),new(2.0f, 1f),
                new(-1.0f, 2f),new(0.0f, 2f),new(1.0f, 2f),
                new(-1.0f, 3f),new(2.0f, 3f),
                new(0.0f, 4f),new(1.0f, 4f),

            };

            // S字母模式
            Data['S'] = new List<Vector2>
            {
                // 从下到上构建
                new(-1.0f, 0f),new(0.0f, 0f),new(1.0f, 0f),
                new(2.0f, 1f),
                new(0.0f, 2f),new(1.0f, 2f),
                new(-1.0f, 3f),
                new(0.0f, 4f),new(1.0f, 4f),new(2.0f, 4f),

            };

            // T字母模式
            Data['T'] = new List<Vector2>
            {
                // 从下到上构建
                new(0.5f, 0f),
                new(0.5f, 1f),
                new(0.5f, 2f),
                new(0.5f, 3f),
                new(-0.5f, 4f),new(0.5f, 4f),new(1.5f, 4f),

            };

            // U字母模式
            Data['U'] = new List<Vector2>
            {
                // 从下到上构建
                new(0.0f, 0f),new(1.0f, 0f),
                new(-1.0f, 1f),new(2.0f, 1f),
                new(-1.0f, 2f),new(2.0f, 2f),
                new(-1.0f, 3f),new(2.0f, 3f),
                new(-1.0f, 4f),new(2.0f, 4f),

            };

            // V字母模式
            Data['V'] = new List<Vector2>
            {
                // 从下到上构建
                new(0.5f, 0f),
                new(-0.5f, 1f),new(1.5f, 1f),
                new(-1.5f, 2f),new(2.5f, 2f),
                new(-1.5f, 3f),new(2.5f, 3f),
                new(-1.5f, 4f),new(2.5f, 4f),

            };

            // W字母模式
            Data['W'] = new List<Vector2>
            {
                // 从下到上构建
                new(-0.5f, 0f),new(0.5f, 0f),new(1.5f, 0f),
                new(-1.5f, 1f),new(0.5f, 1f),new(2.5f, 1f),
                new(-1.5f, 2f),new(0.5f, 2f),new(2.5f, 2f),
                new(-1.5f, 3f),new(0.5f, 3f),new(2.5f, 3f),
                new(-1.5f, 4f),new(2.5f, 4f),

            };

            // X字母模式
            Data['X'] = new List<Vector2>
            {
                // 从下到上构建
                new(-0.5f, 0f),new(1.5f, 0f),
                new(-0.5f, 1f),new(1.5f, 1f),
                new(0.5f, 2f),
                new(-0.5f, 3f),new(1.5f, 3f),
                new(-0.5f, 4f),new(1.5f, 4f),

            };

            // Y字母模式
            Data['Y'] = new List<Vector2>
            {
                // 从下到上构建
                new(0.5f, 0f),
                new(0.5f, 1f),
                new(-0.5f, 2f),new(0.5f, 2f),new(1.5f, 2f),
                new(-0.5f, 3f),new(1.5f, 3f),
                new(-0.5f, 4f),new(1.5f, 4f),

            };

            // Z字母模式
            Data['Z'] = new List<Vector2>
            {
                // 从下到上构建
                new(-1.0f, 0f),new(0.0f, 0f),new(1.0f, 0f),new(2.0f, 0f),
                new(0.0f, 1f),
                new(1.0f, 2f),
                new(2.0f, 3f),
                new(-1.0f, 4f),new(0.0f, 4f),new(1.0f, 4f),new(2.0f, 4f),

            };

            // !字母模式
            Data['!'] = new List<Vector2>
            {
                // 从下到上构建
                new(0.5f, 0f),

                new(0.5f, 2f),
                new(0.5f, 3f),
                new(0.5f, 4f),

            };

            // ?字母模式
            Data['?'] = new List<Vector2>
            {
                // 从下到上构建
                new(0.5f, 0f),

                new(0.5f, 2f),
                new(1.5f, 3f),
                new(-0.5f, 4f),new(0.5f, 4f),new(1.5f, 4f),

            };

            // 0字母模式
            Data['0'] = new List<Vector2>
            {
                // 从下到上构建
                new(0.5f, 0f),
                new(-0.5f, 1f),new(1.5f, 1f),
                new(-0.5f, 2f),new(1.5f, 2f),
                new(-0.5f, 3f),new(1.5f, 3f),
                new(0.5f, 4f),

            };

            // 1字母模式
            Data['1'] = new List<Vector2>
            {
                // 从下到上构建
                new(-0.5f, 0f),new(0.5f, 0f),new(1.5f, 0f),
                new(0.5f, 1f),
                new(0.5f, 2f),
                new(-0.5f, 3f),new(0.5f, 3f),
                new(0.5f, 4f),

            };

            // 2字母模式
            Data['2'] = new List<Vector2>
            {
                // 从下到上构建
                new(-0.5f, 0f),new(0.5f, 0f),new(1.5f, 0f),
                new(-0.5f, 1f),
                new(0.5f, 2f),new(1.5f, 2f),
                new(1.5f, 3f),
                new(-0.5f, 4f),new(0.5f, 4f),

            };

            // 3字母模式
            Data['3'] = new List<Vector2>
            {
                // 从下到上构建
                new(-0.5f, 0f),new(0.5f, 0f),
                new(1.5f, 1f),
                new(-0.5f, 2f),new(0.5f, 2f),
                new(1.5f, 3f),
                new(-0.5f, 4f),new(0.5f, 4f),

            };

            // 4字母模式
            Data['4'] = new List<Vector2>
            {
                // 从下到上构建
                new(1.0f, 0f),
                new(-1.0f, 1f),new(0.0f, 1f),new(1.0f, 1f),new(2.0f, 1f),
                new(-1.0f, 2f),new(1.0f, 2f),
                new(0.0f, 3f),new(1.0f, 3f),
                new(1.0f, 4f),

            };

            // 5字母模式
            Data['5'] = new List<Vector2>
            {
                // 从下到上构建
                new(-0.5f, 0f),new(0.5f, 0f),
                new(1.5f, 1f),
                new(-0.5f, 2f),new(0.5f, 2f),
                new(-0.5f, 3f),
                new(-0.5f, 4f),new(0.5f, 4f),new(1.5f, 4f),

            };

            // 6字母模式
            Data['6'] = new List<Vector2>
            {
                // 从下到上构建
                new(0.0f, 0f),new(1.0f, 0f),
                new(-1.0f, 1f),new(2.0f, 1f),
                new(-1.0f, 2f),new(0.0f, 2f),new(1.0f, 2f),
                new(-1.0f, 3f),
                new(0.0f, 4f),new(1.0f, 4f),

            };

            // 7字母模式
            Data['7'] = new List<Vector2>
            {
                // 从下到上构建
                new(1.5f, 0f),
                new(1.5f, 1f),
                new(1.5f, 2f),
                new(-0.5f, 3f),new(1.5f, 3f),
                new(-0.5f, 4f),new(0.5f, 4f),new(1.5f, 4f),

            };

            // 8字母模式
            Data['8'] = new List<Vector2>
            {
                // 从下到上构建
                new(0.0f, 0f),new(1.0f, 0f),
                new(-1.0f, 1f),new(2.0f, 1f),
                new(0.0f, 2f),new(1.0f, 2f),
                new(-1.0f, 3f),new(2.0f, 3f),
                new(0.0f, 4f),new(1.0f, 4f),

            };

            // 9字母模式
            Data['9'] = new List<Vector2>
            {
                // 从下到上构建
                new(0.0f, 0f),new(1.0f, 0f),
                new(2.0f, 1f),
                new(0.0f, 2f),new(1.0f, 2f),new(2.0f, 2f),
                new(-1.0f, 3f),new(2.0f, 3f),
                new(0.0f, 4f),new(1.0f, 4f),

            };
        }
    }
}