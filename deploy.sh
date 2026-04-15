#!/bin/bash

# Цвета для вывода
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo "========================================"
echo -e "${GREEN}   🚀 ДЕПЛОЙ DINO_MIR (LINUX/MAC)${NC}"
echo "========================================"
echo ""

echo -e "${BLUE}[1/5]${NC} 📦 Остановка старых контейнеров..."
docker-compose down 2>/dev/null
if [ $? -ne 0 ]; then
    echo -e "${YELLOW}Старых контейнеров не найдено, продолжаем...${NC}"
fi
echo ""

echo -e "${BLUE}[2/5]${NC} 🧹 Очистка неиспользуемых образов..."
docker image prune -f
echo ""

echo -e "${BLUE}[3/5]${NC} 🔨 Сборка нового образа..."
docker-compose build --no-cache
if [ $? -ne 0 ]; then
    echo -e "${RED}❌ ОШИБКА: Не удалось собрать образ!${NC}"
    exit 1
fi
echo ""

echo -e "${BLUE}[4/5]${NC} ▶️ Запуск приложения..."
docker-compose up -d
if [ $? -ne 0 ]; then
    echo -e "${RED}❌ ОШИБКА: Не удалось запустить контейнер!${NC}"
    exit 1
fi
echo ""

echo -e "${BLUE}[5/5]${NC} 📊 Статус контейнеров:"
docker-compose ps
echo ""

echo "========================================"
echo -e "${GREEN}   ✅ ДЕПЛОЙ УСПЕШНО ЗАВЕРШЁН!${NC}"
echo "========================================"
echo ""
echo -e "🌐 Приложение доступно по адресу: ${YELLOW}http://localhost:8080${NC}"
echo ""
echo "📋 Полезные команды:"
echo "   - Посмотреть логи: docker-compose logs -f"
echo "   - Остановить: docker-compose down"
echo "   - Перезапустить: docker-compose restart"
echo ""