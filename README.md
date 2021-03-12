How to store multilingual content.

Let's think about it.

## Prerequirement

- Azure Storage Account
- Azure Tranlsator


## Requirement

- .NET 5
- ASP.NET Core
- Entity Framework Core
- RDBMS (here SQL Server)


## Entities

> on SQL Server

```sql
CREATE TABLE [dbo].[Languages] (
    [Id]          NVARCHAR (450)     NOT NULL,
    [Code]        NVARCHAR (40)      DEFAULT (N'') NOT NULL,
    [Name]        NVARCHAR (MAX)     NULL,
    [Description] NVARCHAR (MAX)     NULL,
    [IsDeleted]   BIT                NOT NULL,
    [CreatedAt]   DATETIMEOFFSET (7) NOT NULL,
    [UpdatedAt]   DATETIMEOFFSET (7) NOT NULL,
    [DeletedAt]   DATETIMEOFFSET (7) NULL,
    CONSTRAINT [PK_Languages] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_Languages_Code]
    ON [dbo].[Languages]([Code] ASC);
GO

CREATE TABLE [dbo].[Posts] (
    [Id]        NVARCHAR (450)     NOT NULL,
    [TitleId]   NVARCHAR (450)     NULL,
    [ContentId] NVARCHAR (450)     NULL,
    [IsDeleted] BIT                NOT NULL,
    [CreatedAt] DATETIMEOFFSET (7) NOT NULL,
    [UpdatedAt] DATETIMEOFFSET (7) NOT NULL,
    [DeletedAt] DATETIMEOFFSET (7) NULL,
    CONSTRAINT [PK_Posts] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Posts_LocalizationSet_ContentId] FOREIGN KEY ([ContentId]) REFERENCES [dbo].[LocalizationSet] ([Id]),
    CONSTRAINT [FK_Posts_LocalizationSet_TitleId] FOREIGN KEY ([TitleId]) REFERENCES [dbo].[LocalizationSet] ([Id])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Posts_ContentId]
    ON [dbo].[Posts]([ContentId] ASC) WHERE ([ContentId] IS NOT NULL);
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_Posts_TitleId]
    ON [dbo].[Posts]([TitleId] ASC) WHERE ([TitleId] IS NOT NULL);
GO

CREATE TABLE [dbo].[LocalizationSet] (
    [Id]        NVARCHAR (450)     NOT NULL,
    [IsDeleted] BIT                NOT NULL,
    [CreatedAt] DATETIMEOFFSET (7) NOT NULL,
    [UpdatedAt] DATETIMEOFFSET (7) NOT NULL,
    [DeletedAt] DATETIMEOFFSET (7) NULL,
    CONSTRAINT [PK_LocalizationSet] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

CREATE TABLE [dbo].[Localizations] (
    [Id]         NVARCHAR (450)     NOT NULL,
    [LanguageId] NVARCHAR (450)     NOT NULL,
    [Value]      NVARCHAR (MAX)     NULL,
    [IsDeleted]  BIT                NOT NULL,
    [CreatedAt]  DATETIMEOFFSET (7) NOT NULL,
    [UpdatedAt]  DATETIMEOFFSET (7) NOT NULL,
    [DeletedAt]  DATETIMEOFFSET (7) NULL,
    CONSTRAINT [PK_Localizations] PRIMARY KEY CLUSTERED ([Id] ASC, [LanguageId] ASC),
    CONSTRAINT [FK_Localizations_Languages_LanguageId] FOREIGN KEY ([LanguageId]) REFERENCES [dbo].[Languages] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Localizations_LocalizationSet_Id] FOREIGN KEY ([Id]) REFERENCES [dbo].[LocalizationSet] ([Id]) ON DELETE CASCADE
);
GO

CREATE NONCLUSTERED INDEX [IX_Localizations_LanguageId]
    ON [dbo].[Localizations]([LanguageId] ASC);
GO
```

## Specifications

Base URL: `<scheme>://<host>:<port>/api/v1.1`

e.g.) https://localhost:5001/api/v1.1/languages

### Support languages

Request URI:

`GET:/languages`

Response body:

```json
{
  "data": [
    {
      "id": "88352250-8723-4b50-9708-f8e1d5517089",
      "code": "en",
      "name": "English"
    },
    {
      "id": "9735f5bd-c7e1-4c13-b41e-e60895b188e8",
      "code": "es",
      "name": "española"
    },
    {
      "id": "f592dd7b-c857-4a1c-8123-4d704edc6222",
      "code": "ru",
      "name": "русский"
    },
    {
      "id": "003e7de1-a558-414c-9232-02419adcca0e",
      "code": "zh-Hant",
      "name": "中国传统的"
    },
    {
      "id": "a14dccef-632d-4bc9-b1d5-93b00c089ce1",
      "code": "ko",
      "name": "한국어"
    },
    {
      "id": "868402ea-5991-4bf6-a275-09f0f7ae4232",
      "code": "ja",
      "name": "日本語"
    },
    {
      "id": "62892865-d2d0-4789-aae5-fece088d30ef",
      "code": "zh-Hans",
      "name": "简体中文"
    }
  ],
  "statusCode": 200,
  "message": ""
}
```

### Post list

Request URI:
`GET:/posts`

Querystring:

- language: string (required) Set post title and content language. Language code such as 'ko, en, es, ...'. See Support languages section.
- page: number (optional) default=1
- take: number (optional) default=10

Response body:

```json
{
  "data": [
    {
      "id": "69450270-b57d-4252-beb9-27fd4f1d86d8",
      "title": "확인을 위한 게시물 #5",
      "content": "[5] 신축 한양대 한마음창원병원이 2일 개원과 동시에 진료를 시작했다. 창원시 의창구 사림동 일대 10만9440㎡(약 3만7000평) 면적에 지하 4층 지상 10층 규모의 새 보금자리에서 재출발한 병원은 12개 병동에 총 30개 진료과 25개 센터에서 100명의 전문의가 진료를 본다. 병원 측은 지난해 12월 실내 일부 공사가 완료된 시점부터 모의진료 등을 통해 진료체계와 시스템 안전성을 점검했으며, 지난 2월 28일 상남동 소재 병원 입원환자 200여 명을 신축 병원으로 이송하는 것으로 입원진료를 시작했다. 병원은 창원시로부터 473개 병상 운영으로 1차 허가를 받았으며, 순차적으로 최대 수용 가능한 1008개 병상까지 늘려갈 계획이다."
    },
    {
      "id": "50075b93-44de-46fe-a0a9-b593670a53e6",
      "title": "확인을 위한 게시물 #3",
      "content": "[3] 신축 한양대 한마음창원병원이 2일 개원과 동시에 진료를 시작했다. 창원시 의창구 사림동 일대 10만9440㎡(약 3만7000평) 면적에 지하 4층 지상 10층 규모의 새 보금자리에서 재출발한 병원은 12개 병동에 총 30개 진료과 25개 센터에서 100명의 전문의가 진료를 본다. 병원 측은 지난해 12월 실내 일부 공사가 완료된 시점부터 모의진료 등을 통해 진료체계와 시스템 안전성을 점검했으며, 지난 2월 28일 상남동 소재 병원 입원환자 200여 명을 신축 병원으로 이송하는 것으로 입원진료를 시작했다. 병원은 창원시로부터 473개 병상 운영으로 1차 허가를 받았으며, 순차적으로 최대 수용 가능한 1008개 병상까지 늘려갈 계획이다."
    },
    {
      "id": "ecfb2dff-43dd-4275-ac28-ced27a1c8c6b",
      "title": "확인을 위한 게시물 #2",
      "content": "[2] 신축 한양대 한마음창원병원이 2일 개원과 동시에 진료를 시작했다. 창원시 의창구 사림동 일대 10만9440㎡(약 3만7000평) 면적에 지하 4층 지상 10층 규모의 새 보금자리에서 재출발한 병원은 12개 병동에 총 30개 진료과 25개 센터에서 100명의 전문의가 진료를 본다. 병원 측은 지난해 12월 실내 일부 공사가 완료된 시점부터 모의진료 등을 통해 진료체계와 시스템 안전성을 점검했으며, 지난 2월 28일 상남동 소재 병원 입원환자 200여 명을 신축 병원으로 이송하는 것으로 입원진료를 시작했다. 병원은 창원시로부터 473개 병상 운영으로 1차 허가를 받았으며, 순차적으로 최대 수용 가능한 1008개 병상까지 늘려갈 계획이다."
    }
  ],
  "statusCode": 200,
  "message": ""
}
```

### Post detail

Request URI:
`GET:/posts/:id`

Params:

- id: string (required)

Querystring:

- language: string (optional) Set post title and content language. Language code such as 'ko, en, es, ...'. If language does not set, you get all supported language content. See Support languages section.

Response body:

```json
{
  "data": {
    "id": "ecfb2dff-43dd-4275-ac28-ced27a1c8c6b",
    "contetents": [
      {
        "title": "發佈用於驗證#2",
        "content": "[2] 新建的漢陽大學同心昌源醫院2日與開院同時開始就診。在昌原市宜昌區沙林洞一帶10.9440萬平方米（約3.7萬坪）面積，從地下4層地上10層的新寶所重新出發的醫院，在12個病房共30個診療，在25個中心由100名專科醫生進行診療。醫院方面從去年12月完成室內部分工程時起，通過類比診療等方式檢查了診療體系和系統安全性，並於2月28日將上南洞的200多名醫院住院患者送往新醫院，開始了住院治療。醫院已經從昌原市獲得了473張病床的運營第一次許可，計劃依次增加到最大能接受的1008張病床。",
        "languageCode": "zh-Hant"
      },
      {
        "title": "发布用于验证#2",
        "content": "[2] 新建的汉阳大学同心昌源医院2日与开院同时开始就诊。在昌原市宜昌区沙林洞一带10.9440万平方米（约3.7万坪）面积，从地下4层地上10层的新宝所重新出发的医院，在12个病房共30个诊疗，在25个中心由100名专科医生进行诊疗。医院方面从去年12月完成室内部分工程时起，通过模拟诊疗等方式检查了诊疗体系和系统安全性，并于2月28日将上南洞的200多名医院住院患者送往新医院，开始了住院治疗。医院已经从昌原市获得了473张病床的运营第一次许可，计划依次增加到最大能接受的1008张病床。",
        "languageCode": "zh-Hans"
      },
      {
        "title": "#2の投稿",
        "content": "新しい漢陽大学ハンマウム昌原病院は2月2日に開院したのと同時に治療を開始した。.昌原市のサリム洞の地下4階10階(約37,000平)の新しいねぐらで再開された病院では、25の区で合計30件の治療を受け、15のセンターで合計30件の治療を受けています。昨年12月に一部の屋内工事が完了した時点から、模擬診療等を通じて医療システムの安全性を確認し、2月28日にはサンナム洞の入院患者200人以上が新しい病院に搬送して入院治療を開始しました。病院は長原市から473床の手術を受ける一次許可を受けており、ベッド数を1008床まで順次収容できる1008床に増やす計画です。",
        "languageCode": "ja"
      },
      {
        "title": "Post for #2",
        "content": "[2] The new Hanyang University HanmaeumChangwon Hospital began treatment at the same time as it was open on February 2. The hospital, which has been re-started in a new roost on the 10th floor of the 4th basement floor on 10,000 square meters(about 37,000 pyeong) area in Sarim-dong, U.S. District, Changwon City, sees a total of 30 medical treatments in 12 wards and 100 specialists in 25 centers. The hospital checked the safety of the medical system and system through mock care, etc. from the time some indoor construction was completed in December last year, and on February 28, more than 200 hospital inpatients in Sangnam-dong began inpatient treatment by transporting them to a new hospital. The hospital has received a primary permit from Changwon City to operate 473 beds, and plans to increase the number of beds to 1008 hospital beds that can be accommodated up to 1008 sequentially.",
        "languageCode": "en"
      },
      {
        "title": "Publicación para #2",
        "content": "[2] El nuevo Hospital HanmaeumChangwon de la Universidad de Hanyang comenzó el tratamiento al mismo tiempo que estaba abierto el 2 de febrero. El hospital, que ha sido re-iniciado en un nuevo dormidero en el piso 10 del 4º piso del sótano en 10.000 metros cuadrados (unos 37.000 pyeong) área en Sarim-dong, distrito de ee.UU., Changwon City, ve un total de 30 tratamientos médicos en 12 pabellones y 100 especialistas en 25 centros. El hospital revisó la seguridad del sistema y el sistema médico a través de la atención simulada, etc. desde el momento en que se completó alguna construcción en interiores en diciembre del año pasado, y el 28 de febrero, más de 200 pacientes hospitalizados en Sangnam-dong comenzaron el tratamiento hospitalario transportándolos a un nuevo hospital. El hospital ha recibido un permiso primario de la ciudad de Changwon para operar 473 camas, y planea aumentar el número de camas a 1008 camas de hospital que se pueden acomodar hasta 1008 secuencialmente.",
        "languageCode": "es"
      },
      {
        "title": "확인을 위한 게시물 #2",
        "content": "[2] 신축 한양대 한마음창원병원이 2일 개원과 동시에 진료를 시작했다. 창원시 의창구 사림동 일대 10만9440㎡(약 3만7000평) 면적에 지하 4층 지상 10층 규모의 새 보금자리에서 재출발한 병원은 12개 병동에 총 30개 진료과 25개 센터에서 100명의 전문의가 진료를 본다. 병원 측은 지난해 12월 실내 일부 공사가 완료된 시점부터 모의진료 등을 통해 진료체계와 시스템 안전성을 점검했으며, 지난 2월 28일 상남동 소재 병원 입원환자 200여 명을 신축 병원으로 이송하는 것으로 입원진료를 시작했다. 병원은 창원시로부터 473개 병상 운영으로 1차 허가를 받았으며, 순차적으로 최대 수용 가능한 1008개 병상까지 늘려갈 계획이다.",
        "languageCode": "ko"
      },
      {
        "title": "Сообщение для #2",
        "content": "Новая больница Hanyang University HanmaeumChangwon начала лечение в то же время, что и 2 февраля. Больница, которая была вновь запущена в новый насест на 10-м этаже 4-го цокольного этажа на 10000 квадратных метров (около 37000 пхеон) области в Сарим-дон, округ США, Changwon City, видит в общей сложности 30 медицинских процедур в 12 палатах и 100 специалистов в 25 центрах. Больница проверила безопасность медицинской системы и системы с помощью макетной помощи и т.д. С того времени, когда в декабре прошлого года было завершено строительство помещений, более 200 стационарных пациентов больницы в Сангнам-доне начали стационарное лечение, перевозя их в новую больницу. Больница получила первичное разрешение от города Чанвон на эксплуатацию 473 коек и планирует увеличить количество коек до 1008 больничных коек, которые могут быть размещены до 1008 последовательно.",
        "languageCode": "ru"
      }
    ]
  },
  "statusCode": 200,
  "message": ""
}
```

### Add post

Request URI:
`POST:/posts`

Request body:
Schema:

```
{
  criteriaLanguageCode: string (Requred) If use translation, post contents can be multiple, so need to specify the criteria language.
  postContents: [
    {
      title: string (Required) The post title
      content: string (Required) The post content
      languageCode: string (Required) The post language written by
    }
  ],
  useTranslation: boolean (optional) Use translation
  isHtmlContent: boolean (optional) The post content is html or not
  isTranslationEachLanguage: boolean (optional) [Deprecated] 
}
```

e.g.)

```json
{
  "criteriaLanguageCode": "ko",
  "postContents": [
    {
      "title": "확인을 위한 게시물 #7",
      "content": "[7] 신축 한양대 한마음창원병원이 2일 개원과 동시에 진료를 시작했다. 창원시 의창구 사림동 일대 10만9440㎡(약 3만7000평) 면적에 지하 4층 지상 10층 규모의 새 보금자리에서 재출발한 병원은 12개 병동에 총 30개 진료과 25개 센터에서 100명의 전문의가 진료를 본다. 병원 측은 지난해 12월 실내 일부 공사가 완료된 시점부터 모의진료 등을 통해 진료체계와 시스템 안전성을 점검했으며, 지난 2월 28일 상남동 소재 병원 입원환자 200여 명을 신축 병원으로 이송하는 것으로 입원진료를 시작했다. 병원은 창원시로부터 473개 병상 운영으로 1차 허가를 받았으며, 순차적으로 최대 수용 가능한 1008개 병상까지 늘려갈 계획이다.",
      "languageCode": "ko"
    }
  ],
  "useTranslation": true,
  "isHtmlContent": false,
  "isTranslationEachLanguage": false
}
```

Response body:

```json
{
  "data": {
    "id": "716d7cd2-433e-4ed3-a852-7e6729c3b34e",
    "contetents": [
      {
        "title": "發佈帖子以#7",
        "content": "[7] 新建的漢陽大學同心昌源醫院2日開診同時開始。在昌原市宜昌區沙林洞一帶10.9440萬平方米（約3.7萬坪）面積，從地下4層地上10層的新寶所重新出發的醫院，在12個病房共30個診療，在25個中心由100名專科醫生進行診療。醫院方面從去年12月完成室內部分工程時起，通過類比診療等方式檢查了診療體系和系統安全性，並於2月28日將上南洞的200多名醫院住院患者送往新醫院，開始了住院治療。醫院已經從昌原市獲得了473張病床的運營第一次許可，計劃依次增加到最大能接受的1008張病床。",
        "languageCode": "zh-Hant"
      },
      {
        "title": "发布帖子以#7",
        "content": "[7] 新建的汉阳大学同心昌源医院2日开诊同时开始。在昌原市宜昌区沙林洞一带10.9440万平方米（约3.7万坪）面积，从地下4层地上10层的新宝所重新出发的医院，在12个病房共30个诊疗，在25个中心由100名专科医生进行诊疗。医院方面从去年12月完成室内部分工程时起，通过模拟诊疗等方式检查了诊疗体系和系统安全性，并于2月28日将上南洞的200多名医院住院患者送往新医院，开始了住院治疗。医院已经从昌原市获得了473张病床的运营第一次许可，计划依次增加到最大能接受的1008张病床。",
        "languageCode": "zh-Hans"
      },
      {
        "title": "#7の投稿",
        "content": "新しい漢陽大学ハンマウム昌原病院は2月2日に開院したのと同時に治療を開始した。.昌原市のサリム洞の地下4階10階(約37,000平)の新しいねぐらで再開された病院では、25の区で合計30件の治療を受け、15のセンターで合計30件の治療を受けています。昨年12月に一部の屋内工事が完了した時点から、模擬診療等を通じて医療システムの安全性を確認し、2月28日にはサンナム洞の入院患者200人以上が新しい病院に搬送して入院治療を開始しました。病院は長原市から473床の手術を受ける一次許可を受けており、ベッド数を1008床まで順次収容できる1008床に増やす計画です。",
        "languageCode": "ja"
      },
      {
        "title": "Post for #7",
        "content": "[7] The new Hanyang University HanmaeumChangwon Hospital began treatment at the same time as it was open on February 2. The hospital, which has been re-started in a new roost on the 10th floor of the 4th basement floor on 10,000 square meters(about 37,000 pyeong) area in Sarim-dong, U.S. District, Changwon City, sees a total of 30 medical treatments in 12 wards and 100 specialists in 25 centers. The hospital checked the safety of the medical system and system through mock care, etc. from the time some indoor construction was completed in December last year, and on February 28, more than 200 hospital inpatients in Sangnam-dong began inpatient treatment by transporting them to a new hospital. The hospital has received a primary permit from Changwon City to operate 473 beds, and plans to increase the number of beds to 1008 hospital beds that can be accommodated up to 1008 sequentially.",
        "languageCode": "en"
      },
      {
        "title": "Publicación para #7",
        "content": "[7] El nuevo Hospital HanmaeumChangwon de la Universidad de Hanyang comenzó el tratamiento al mismo tiempo que estaba abierto el 2 de febrero. El hospital, que ha sido re-iniciado en un nuevo dormidero en el piso 10 del 4º piso del sótano en 10.000 metros cuadrados (unos 37.000 pyeong) área en Sarim-dong, distrito de ee.UU., Changwon City, ve un total de 30 tratamientos médicos en 12 pabellones y 100 especialistas en 25 centros. El hospital revisó la seguridad del sistema y el sistema médico a través de la atención simulada, etc. desde el momento en que se completó alguna construcción en interiores en diciembre del año pasado, y el 28 de febrero, más de 200 pacientes hospitalizados en Sangnam-dong comenzaron el tratamiento hospitalario transportándolos a un nuevo hospital. El hospital ha recibido un permiso primario de la ciudad de Changwon para operar 473 camas, y planea aumentar el número de camas a 1008 camas de hospital que se pueden acomodar hasta 1008 secuencialmente.",
        "languageCode": "es"
      },
      {
        "title": "확인을 위한 게시물 #7",
        "content": "[7] 신축 한양대 한마음창원병원이 2일 개원과 동시에 진료를 시작했다. 창원시 의창구 사림동 일대 10만9440㎡(약 3만7000평) 면적에 지하 4층 지상 10층 규모의 새 보금자리에서 재출발한 병원은 12개 병동에 총 30개 진료과 25개 센터에서 100명의 전문의가 진료를 본다. 병원 측은 지난해 12월 실내 일부 공사가 완료된 시점부터 모의진료 등을 통해 진료체계와 시스템 안전성을 점검했으며, 지난 2월 28일 상남동 소재 병원 입원환자 200여 명을 신축 병원으로 이송하는 것으로 입원진료를 시작했다. 병원은 창원시로부터 473개 병상 운영으로 1차 허가를 받았으며, 순차적으로 최대 수용 가능한 1008개 병상까지 늘려갈 계획이다.",
        "languageCode": "ko"
      },
      {
        "title": "Сообщение для #7",
        "content": "Новая больница Hanyang University HanmaeumChangwon начала лечение в то же время, что и 2 февраля. Больница, которая была вновь запущена в новый насест на 10-м этаже 4-го цокольного этажа на 10000 квадратных метров (около 37000 пхеон) области в Сарим-дон, округ США, Changwon City, видит в общей сложности 30 медицинских процедур в 12 палатах и 100 специалистов в 25 центрах. Больница проверила безопасность медицинской системы и системы с помощью макетной помощи и т.д. С того времени, когда в декабре прошлого года было завершено строительство помещений, более 200 стационарных пациентов больницы в Сангнам-доне начали стационарное лечение, перевозя их в новую больницу. Больница получила первичное разрешение от города Чанвон на эксплуатацию 473 коек и планирует увеличить количество коек до 1008 больничных коек, которые могут быть размещены до 1008 последовательно.",
        "languageCode": "ru"
      }
    ]
  },
  "statusCode": 201,
  "message": ""
}
```

### Update post

Request URI:
`PATCH:/posts/:id`

Params:

- id: string (Required)

Request body:
Schema:

```
{
  id: string (Required) The post identifier
  criteriaLanguageCode: string (Requred) If use translation, post contents can be multiple, so need to specify the criteria language.
  postContents: [
    {
      title: string (Required) The post title
      content: string (Required) The post content
      languageCode: string (Required) The post language written by
    }
  ],
  useTranslation: boolean (optional) Use translation
  isHtmlContent: boolean (optional) The post content is html or not
  isTranslationEachLanguage: boolean (optional) [Deprecated] 
}
```

e.g.)

```json
{
  "id": "69450270-b57d-4252-beb9-27fd4f1d86d8",
  "criteriaLanguageCode": "ko",
  "postContents": [
    {
      "title": "확인을 위한 게시물 #8",
      "content": "[8] 신축 한양대 한마음창원병원이 2일 개원과 동시에 진료를 시작했다. 창원시 의창구 사림동 일대 10만9440㎡(약 3만7000평) 면적에 지하 4층 지상 10층 규모의 새 보금자리에서 재출발한 병원은 12개 병동에 총 30개 진료과 25개 센터에서 100명의 전문의가 진료를 본다. 병원 측은 지난해 12월 실내 일부 공사가 완료된 시점부터 모의진료 등을 통해 진료체계와 시스템 안전성을 점검했으며, 지난 2월 28일 상남동 소재 병원 입원환자 200여 명을 신축 병원으로 이송하는 것으로 입원진료를 시작했다. 병원은 창원시로부터 473개 병상 운영으로 1차 허가를 받았으며, 순차적으로 최대 수용 가능한 1008개 병상까지 늘려갈 계획이다.",
      "languageCode": "ko"
    }
  ],
  "useTranslation": true,
  "isHtmlContent": false,
  "isTranslationEachLanguage": false
}
```

Response body:

```json
{
  "data": {
    "id": "69450270-b57d-4252-beb9-27fd4f1d86d8",
    "contetents": [
      {
        "title": "發佈帖子以#8",
        "content": "[8] 新建的漢陽大學同心昌原醫院2日與開院同時開診。在昌原市宜昌區沙林洞一帶10.9440萬平方米（約3.7萬坪）面積，從地下4層地上10層的新寶所重新出發的醫院，在12個病房共30個診療，在25個中心由100名專科醫生進行診療。醫院方面從去年12月完成室內部分工程時起，通過類比診療等方式檢查了診療體系和系統安全性，並於2月28日將上南洞的200多名醫院住院患者送往新醫院，開始了住院治療。醫院已經從昌原市獲得了473張病床的運營第一次許可，計劃依次增加到最大能接受的1008張病床。",
        "languageCode": "zh-Hant"
      },
      {
        "title": "发布帖子以#8",
        "content": "[8] 新建的汉阳大学同心昌原医院2日与开院同时开诊。在昌原市宜昌区沙林洞一带10.9440万平方米（约3.7万坪）面积，从地下4层地上10层的新宝所重新出发的医院，在12个病房共30个诊疗，在25个中心由100名专科医生进行诊疗。医院方面从去年12月完成室内部分工程时起，通过模拟诊疗等方式检查了诊疗体系和系统安全性，并于2月28日将上南洞的200多名医院住院患者送往新医院，开始了住院治疗。医院已经从昌原市获得了473张病床的运营第一次许可，计划依次增加到最大能接受的1008张病床。",
        "languageCode": "zh-Hans"
      },
      {
        "title": "#8の投稿",
        "content": "新しい漢陽大学ハンマウム昌原病院は2月2日に開院したのと同時に治療を開始した。.昌原市のサリム洞の地下4階10階(約37,000平)の新しいねぐらで再開された病院では、25の区で合計30件の治療を受け、15のセンターで合計30件の治療を受けています。昨年12月に一部の屋内工事が完了した時点から、模擬診療等を通じて医療システムの安全性を確認し、2月28日にはサンナム洞の入院患者200人以上が新しい病院に搬送して入院治療を開始しました。病院は長原市から473床の手術を受ける一次許可を受けており、ベッド数を1008床まで順次収容できる1008床に増やす計画です。",
        "languageCode": "ja"
      },
      {
        "title": "Post for #8",
        "content": "[8] The new Hanyang University HanmaeumChangwon Hospital began treatment at the same time as it was open on February 2. The hospital, which has been re-started in a new roost on the 10th floor of the 4th basement floor on 10,000 square meters(about 37,000 pyeong) area in Sarim-dong, U.S. District, Changwon City, sees a total of 30 medical treatments in 12 wards and 100 specialists in 25 centers. The hospital checked the safety of the medical system and system through mock care, etc. from the time some indoor construction was completed in December last year, and on February 28, more than 200 hospital inpatients in Sangnam-dong began inpatient treatment by transporting them to a new hospital. The hospital has received a primary permit from Changwon City to operate 473 beds, and plans to increase the number of beds to 1008 hospital beds that can be accommodated up to 1008 sequentially.",
        "languageCode": "en"
      },
      {
        "title": "Publicación para #8",
        "content": "[8] El nuevo Hospital HanmaeumChangwon de la Universidad de Hanyang comenzó el tratamiento al mismo tiempo que estaba abierto el 2 de febrero. El hospital, que ha sido re-iniciado en un nuevo dormidero en el piso 10 del 4º piso del sótano en 10.000 metros cuadrados (unos 37.000 pyeong) área en Sarim-dong, distrito de ee.UU., Changwon City, ve un total de 30 tratamientos médicos en 12 pabellones y 100 especialistas en 25 centros. El hospital revisó la seguridad del sistema y el sistema médico a través de la atención simulada, etc. desde el momento en que se completó alguna construcción en interiores en diciembre del año pasado, y el 28 de febrero, más de 200 pacientes hospitalizados en Sangnam-dong comenzaron el tratamiento hospitalario transportándolos a un nuevo hospital. El hospital ha recibido un permiso primario de la ciudad de Changwon para operar 473 camas, y planea aumentar el número de camas a 1008 camas de hospital que se pueden acomodar hasta 1008 secuencialmente.",
        "languageCode": "es"
      },
      {
        "title": "확인을 위한 게시물 #8",
        "content": "[8] 신축 한양대 한마음창원병원이 2일 개원과 동시에 진료를 시작했다. 창원시 의창구 사림동 일대 10만9440㎡(약 3만7000평) 면적에 지하 4층 지상 10층 규모의 새 보금자리에서 재출발한 병원은 12개 병동에 총 30개 진료과 25개 센터에서 100명의 전문의가 진료를 본다. 병원 측은 지난해 12월 실내 일부 공사가 완료된 시점부터 모의진료 등을 통해 진료체계와 시스템 안전성을 점검했으며, 지난 2월 28일 상남동 소재 병원 입원환자 200여 명을 신축 병원으로 이송하는 것으로 입원진료를 시작했다. 병원은 창원시로부터 473개 병상 운영으로 1차 허가를 받았으며, 순차적으로 최대 수용 가능한 1008개 병상까지 늘려갈 계획이다.",
        "languageCode": "ko"
      },
      {
        "title": "Сообщение для #8",
        "content": "Новая больница Hanyang University HanmaeumChangwon начала лечение в то же время, что и 2 февраля. Больница, которая была вновь запущена в новый насест на 10-м этаже 4-го цокольного этажа на 10000 квадратных метров (около 37000 пхеон) области в Сарим-дон, округ США, Changwon City, видит в общей сложности 30 медицинских процедур в 12 палатах и 100 специалистов в 25 центрах. Больница проверила безопасность медицинской системы и системы с помощью макетной помощи и т.д. С того времени, когда в декабре прошлого года было завершено строительство помещений, более 200 стационарных пациентов больницы в Сангнам-доне начали стационарное лечение, перевозя их в новую больницу. Больница получила первичное разрешение от города Чанвон на эксплуатацию 473 коек и планирует увеличить количество коек до 1008 больничных коек, которые могут быть размещены до 1008 последовательно.",
        "languageCode": "ru"
      }
    ]
  },
  "statusCode": 202,
  "message": ""
}
```

### Delete post

Request URI:
`DELETE:/posts/:id`

Params:

- id: string (Required)

Response body:

```json
{
  "data": null,
  "statusCode": 202,
  "message": "The post Deleted. (69450270-b57d-4252-beb9-27fd4f1d86d8)"
}
```
