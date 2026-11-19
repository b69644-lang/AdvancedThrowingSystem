КАК ПОЛУЧИТЬ DLL БЕЗ УСТАНОВКИ VISUAL STUDIO

1. Создай аккаунт на github.com (если его ещё нет).
2. Нажми New repository. Название: AdvancedThrowingSystem. Выбери Public или Private.
3. Распакуй этот ZIP на компьютере или телефоне.
4. Загрузи ВСЁ содержимое папки AdvancedThrowingSystem в корень репозитория, сохранив структуру папок. В корне GitHub должны быть папки .github и AdvancedThrowingSystem? НЕТ: файлы csproj, src, dependencies и README должны лежать в корне репозитория, а workflow — в .github/workflows.
   В этом архиве папка AdvancedThrowingSystem содержит проект. Загружай её СОДЕРЖИМОЕ в корень репозитория, а папку .github — тоже в корень.
5. Открой вкладку Actions. Если GitHub попросит разрешить workflows, нажми I understand my workflows, go ahead and enable them.
6. Выбери Build AdvancedThrowingSystem слева и нажми Run workflow.
7. Дождись зелёной галочки. Внизу страницы открой Artifacts и скачай AdvancedThrowingSystem-DLL.
8. Из скачанного ZIP возьми AdvancedThrowingSystem.dll и помести в RDR2\scripts.

ВАЖНО:
- Это попытка облачной сборки, а не гарантия: если API или исходный код несовместимы, GitHub покажет ошибку сборки.
- В проект включена копия ScriptHookRDRNetAPI.dll, которую ты прислал, только для компиляции. Это не сама DLL мода.
- Сам мод остаётся прототипом: нет полноценных анимаций и настоящего физического callback попаданий.
- Используй только в сюжетном режиме и сделай резервную копию сохранения.
