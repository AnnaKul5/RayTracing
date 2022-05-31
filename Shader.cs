using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace RayTracing
{
    public class ShaderProgram
    {
        public readonly int Handle;

        private readonly Dictionary<string, int> _uniformLocations;

        public ShaderProgram(string vertPath, string fragPath)
        {
            //Компиляция вершинного шейдера
            var vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertPath);
            CompileShader(vertexShader);

            //Компиляция фрагментного шейдера
            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragPath);
            CompileShader(fragmentShader);

            //Их прикрепление и линковка - создание шейдерной программы
            Handle = GL.CreateProgram();
            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);
            LinkProgram(Handle);

            //Открепление и удаление использованных шейдеров,
            //поскольку шейдерная программа уже скомпилирована и содержит всё необходимое
            GL.DetachShader(Handle, vertexShader);
            GL.DetachShader(Handle, fragmentShader);
            GL.DeleteShader(fragmentShader);
            GL.DeleteShader(vertexShader);

            //Запись всех uniform-переменных из шейдеров в словарь
            //(словарь содержит имена переменных и хэндлы на них)

            // Во-первых, мы должны получить количество активных униформ в шейдере.
            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);

            // Далее выделите словарь для хранения местоположений
            _uniformLocations = new Dictionary<string, int>();

            // Цикл по всей униформе
            for (var i = 0; i < numberOfUniforms; i++)
            {
                // получить название этой униформы
                var key = GL.GetActiveUniform(Handle, i, out _, out _);

                // получить местоположение
                var location = GL.GetUniformLocation(Handle, key);

                // а затем добавьте его в словарь
                _uniformLocations.Add(key, location);
            }
        }

        private static void CompileShader(int shader)
        {
            // Пробуем скомпилить шейдер
            GL.CompileShader(shader);

            // Проверяем на ошибки компилирования
            GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);
            if (code != (int)All.True)
            {
                // We can use `GL.GetShaderInfoLog(shader)` to get information about the error.
                var infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"Error occurred whilst compiling Shader({shader}).\n\n{infoLog}");
            }
        }

        private static void LinkProgram(int program)
        {
            // Соединяем с программой
            GL.LinkProgram(program);

            // Проверка на ошибки с линквокй
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var code);
            if (code != (int)All.True)
            {
                // We can use `GL.GetProgramInfoLog(program)` to get information about the error.
                throw new Exception($"Error occurred whilst linking Program({program})");
            }
        }

        // Функция-оболочка, которая включает программу шейдеров.
        public void Use()
        {
            GL.UseProgram(Handle);
        }

        public int GetAttribLocation(string attribName)
        {
            return GL.GetAttribLocation(Handle, attribName);
        }

        public void SetInt(string name, int data)
        {
            GL.UseProgram(Handle);
            GL.Uniform1(_uniformLocations[name], data);
        }

        public void SetFloat(string name, float data)
        {
            GL.UseProgram(Handle);
            GL.Uniform1(_uniformLocations[name], data);
        }

        public void SetMatrix4(string name, Matrix4 data)
        {
            GL.UseProgram(Handle);
            GL.UniformMatrix4(_uniformLocations[name], true, ref data);
        }

        public void SetVector2(string name, Vector2 data)
        {
            GL.UseProgram(Handle);
            GL.Uniform2(_uniformLocations[name], data);
        }

        public void SetVector3(string name, Vector3 data)
        {
            GL.UseProgram(Handle);
            GL.Uniform3(_uniformLocations[name], data);
        }
    }
}