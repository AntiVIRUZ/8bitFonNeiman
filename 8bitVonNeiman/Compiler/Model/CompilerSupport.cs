﻿using System;
using System.Collections;
using System.Text.RegularExpressions;
using _8bitVonNeiman.Controller;

namespace _8bitVonNeiman.Compiler.Model {
    public static class CompilerSupport {

        /// <summary>
        /// Структура, для более удобной работы с регистрами при компиляции.
        /// </summary>
        public struct Register {
            /// <summary>
            /// Номер регистра
            /// </summary>
            public int Number;
            /// <summary>
            /// Тип адресации. True, если прямая, False, если косвенная.
            /// </summary>
            public bool IsDirect;
            /// <summary>
            /// Показывает, должен ли быть изменен регистр при выполении команды.
            /// </summary>
            public bool IsChange;
            /// <summary>
            /// True, если регистр должен быть увеличен, False, если уменьшен.
            /// </summary>
            public bool IsIncrement;
            /// <summary>
            /// True, если регист должен быть изменен после обращения, False, если перед.
            /// </summary>
            public bool IsPostchange;
        }

        public const int MaxFarAddress = (1 << Constants.FarAddressBitsCount) - 1;
        public const int MaxVariableAddress = (1 << Constants.ShortAddressBitsCount) - 1;

        /// <summary>
        /// Приводит переданную строку к 8-битовому адресу. 
        /// Если это имя переменной, возвращает адрес, ассоциированный с этой переменной,
        /// если переменная не объявлена, будет сгенерированно исключение <see cref="CompilerEnvironment"/>.
        /// Если в строке содержится число, будет возвращено это число.
        /// </summary>
        /// <param name="name">Имя переменной или строка с числом.</param>
        /// <param name="env">Окружение компилятора.</param>
        /// <returns>Адрес, ассоциированный с переменной или содержащийся в строке как число.</returns>
        public static int ConvertVariableToAddress(string name, CompilerEnvironment env) {
            try {
                if (name[0] >= '0' && name[0] <= '9') {
                    int address = ConvertToInt(name);
                    if (address > MaxVariableAddress) {
                        throw new OverflowException();
                    }
                    return address;
                } else {
                    int address = env.GetVariableAddress(name);
                    return address;
                }
            } catch (OverflowException) {
                throw new CompilationErrorExcepton($"Адрес не должен превышать {MaxVariableAddress}", env.GetCurrentLine());
            } catch (FormatException) {
                throw new CompilationErrorExcepton("Некорректный адрес метки", env.GetCurrentLine());
            } catch (Exception e) {
                throw new CompilationErrorExcepton("Непредвиденная ошибка при обработке метки", env.GetCurrentLine(), e);
            }
        }

        /// <summary>
        /// Приводит переданную строку к полному, 10-битовому адресу.
        /// Если введен слишком большой адрес или строка не является меткой, 
        /// будет сгенерированно исключение <see cref="CompilerEnvironment"/>.
        /// </summary>
        /// <param name="label">Строка-метка или адрес в памяти.</param>
        /// <param name="env">Текущее окружение компилятора.</param>
        /// <returns>Адресс, на который ссылается метка или который был записан как число, или -1, если использована несуществующая метка.</returns>
        public static int ConvertLabelToFarAddress(string label, CompilerEnvironment env) {
            try {
                if (label[0] >= '0' && label[0] <= '9') {
                    int address = ConvertToInt(label);
                    if (address > MaxFarAddress) {
                        throw new OverflowException();
                    }
                    return address;
                } else {
                    int address = env.GetLabelAddress(label);
                    return address;
                }
            } catch (OverflowException) {
                throw new CompilationErrorExcepton($"Адрес не должен превышать {MaxFarAddress}", env.GetCurrentLine());
            } catch (FormatException) {
                throw new CompilationErrorExcepton("Некорректный адрес метки", env.GetCurrentLine());
            } catch (Exception e) {
                throw new CompilationErrorExcepton("Непредвиденная ошибка при обработке метки", env.GetCurrentLine(), e);
            }
        }

        /// <summary>
        /// Преобразует строку в число. Если число начинается с 0x, то оно интерпретируется как 16-ричное, 
        /// если с 0b, то как двоичное. Если не удается преобразовать число, генерируется исключение.
        /// </summary>
        /// <param name="s">Строка, содержащее число.</param>
        /// <returns>Число, содержащееся в строке.</returns>
        public static int ConvertToInt(string s) {  
            if (s.StartsWith("0x")) {
                return Convert.ToInt32(s.Substring(2), 16);
            }
            if (s.StartsWith("0b")) {
                return Convert.ToInt32(s.Substring(2), 2);
            }
            return Convert.ToInt32(s);
        }

        /// <summary>
        /// Заполняет младшие биты массива битов битами из переданного числа.
        /// </summary>
        /// <param name="highBitArray">Массив бит, в который будут записываться биты. Представляет собой 8 старших разрядов.</param>
        /// <param name="lowBitArray">Массив бит, в который будут записываться биты. Представляет собой 8 младших разрядов.</param>
        /// <param name="number">Число, из которого будут браться биты.</param>
        /// <param name="bitsCount">Количество бит, которое будет записано.</param>
        public static void FillBitArray(BitArray highBitArray, BitArray lowBitArray, int number, int bitsCount) {
            for (int i = 0; i < bitsCount; i++) {
                if (i < 8) {
                    lowBitArray[i] = (number & (1 << i)) != 0;
                } else {
                    highBitArray[i % 8] = (number & (1 << i)) != 0;
                }
            }
        }

        /// <summary>
        /// Проверяет слово на корректность для использования в качестве метки, переменной или аргумента.
        /// </summary>
        /// <param name="word">Слово для проверки на корректность.</param>
        public static bool CheckIdentifierName(string word) {
            return word.Length != 0 && 
                Regex.IsMatch(word, @"^[a-zA-Z0-9_-]+$") && 
                !(word[0] <= '9' && word[0] >= '0');
        }

        /// <summary>
        /// Получает регистр из строки. Возвращает null если не удается выполнить преобразование.
        /// </summary>
        /// <param name="s">Строка, из которой необходимо получить регистр.</param>
        /// <returns>Регистр, полученный из строки. Null, если не удалось совершить преобразование.</returns>
        public static Register? ConvertToRegister(string s) {
            if (s.Length < 2 || s.Length > 4) {
                return null;
            }
            var register = new Register {
                IsChange = false,
                IsDirect = true
            };
            try {
                int counter = 0;
                if (s[counter] == '+') {
                    register.IsChange = true;
                    register.IsIncrement = true;
                    register.IsPostchange = false;
                    counter++;
                } else if (s[counter] == '-') {
                    register.IsChange = true;
                    register.IsIncrement = false;
                    register.IsPostchange = false;
                    counter++;
                }
                if (s[counter] == '@') {
                    register.IsDirect = false;
                    counter++;
                }
                if (s[counter] != 'R' && s[counter] != 'r') {
                    return null;
                }
                counter++;
                register.Number = Convert.ToInt32(s[counter].ToString(), 16);
                counter++;
                if (counter == s.Length) {
                    return register;
                }
                if (s[counter] == '+') {
                    if (register.IsChange) {
                        return null;
                    }
                    register.IsChange = true;
                    register.IsIncrement = true;
                    register.IsPostchange = true;
                    counter++;
                } else if (s[counter] == '-') {
                    if (register.IsChange) {
                        return null;
                    }
                    register.IsChange = true;
                    register.IsIncrement = false;
                    register.IsPostchange = true;
                    counter++;
                }
                if (counter == s.Length) {
                    return register;
                } else {
                    return null;
                }
            } catch {
                return null;
            }
        }
    }
}
