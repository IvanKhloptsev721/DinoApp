@echo off
chcp 65001 >nul
echo ========================================
echo    🚀 ДЕПЛОЙ DINO_MIR (WINDOWS)
echo ========================================
echo.

echo [1/5] 📦 Остановка старых контейнеров...
docker-compose down 2>nul
if %errorlevel% neq 0 (
    echo Старых контейнеров не найдено, продолжаем...
)
echo.

echo [2/5] 🧹 Очистка неиспользуемых образов...
docker image prune -f
echo.

echo [3/5] 🔨 Сборка нового образа...
docker-compose build --no-cache
if %errorlevel% neq 0 (
    echo ❌ ОШИБКА: Не удалось собрать образ!
    pause
    exit /b 1
)
echo.

echo [4/5] ▶️ Запуск приложения...
docker-compose up -d
if %errorlevel% neq 0 (
    echo ❌ ОШИБКА: Не удалось запустить контейнер!
    pause
    exit /b 1
)
echo.

echo [5/5] 📊 Статус контейнеров:
docker-compose ps
echo.

echo ========================================
echo    ✅ ДЕПЛОЙ УСПЕШНО ЗАВЕРШЁН!
echo ========================================
echo.
echo 🌐 Приложение доступно по адресу: http://localhost:8080
echo.
echo 📋 Полезные команды:
echo    - Посмотреть логи: docker-compose logs -f
echo    - Остановить: docker-compose down
echo    - Перезапустить: docker-compose restart
echo.
pause