# Asl Belgisi (CRPT / xtrace) — integratsiya holati

> **Holat: KOD TAYYOR — real E2E tashqi credentials/qiymatlarni kutadi.**
> Bu hujjat halol yakuniy bahoni, KUTISH NUQTALARINI (kim beradi, qayerga qo'yiladi) va
> credentials kelganda bajariladigan real E2E qadamlarini beradi.
>
> Oxirgi tekshiruv: build **0 error / 0 warning**, butun suite **190/190**, AslBelgi **20/20**.

---

## 1. Modul holati (halol baho)

| Ko'rsatkich | Holat |
|---|---|
| **Kod tayyorligi** | **~90%** (kod + unit test yashil) |
| **Real API bilan isbotlangan** | **0%** — credentials va real GTIN yo'q, hech qanday jonli so'rov qilinmagan |
| Build | 0 error / 0 warning |
| Testlar | Butun suite 190/190; AslBelgi 20 (token 6 · order 8 · marking 6) |
| Orphaned kod | Yo'q — `AslBelgiAuthClient` endi `AslBelgiTokenProvider` orqali ulangan |

> ⚠️ **"100% ishlaydi" DEYILMAYDI.** Kod va unit testlar yashil, lekin jonli Asl Belgisi
> serveriga bironta real so'rov yuborilmagan (credentials + GTIN kutilmoqda).

### Nima tayyor (kod + test bilan)
- **A1 — Token lifecycle:** `IAslBelgiTokenProvider` (technical: login/parol → token, avto-refresh → re-auth; business: apiKey). Thread-safe cache, safety margin. Refresh `application/x-www-form-urlencoded` (dok bo'yicha tuzatilgan).
- **A2 — Order/codes:** konkret DTO'lar + real endpoint (`/api/orders`, `/api/codes`). `RegisterOrderAsync` / `GetOrdersAsync` / `GetCodesAsync`.
- **A3 — Marking biznes-logikasi:** `RequestMarkingAsync` (order) → `FetchAndBindCodesAsync` (KM → `ProductTable.MarkingNumber`, UoW tranzaksiya, idempotent — `ux_inv_product_table_org_marking` UNIQUE).
- **A4 — DRY refaktor:** `AslBelgiHttpClientBase` (retry/backoff/correlation/BuildUri bir nusxada). Xulq o'zgarmagan.

### Nima isbotlanmagan (real API kutadi)
- Jonli `authenticate`/`refresh` (login/parol yoki apiKey bilan)
- Jonli `POST /api/orders` (real GTIN + справочник qiymatlari bilan)
- Jonli `GET /api/codes` выгрузка va ProductTable'ga yozilishi
- Response strukturasining aniq mosligi (order-list `AslBelgiOrderInfo` — TODO bilan belgilangan)

---

## 2. KUTISH NUQTALARI (kim beradi · qayerga qo'yiladi)

> ASP.NET Core config kaliti env o'zgaruvchisiga `__` (ikki pastki chiziq) bilan o'giriladi.
> Masalan `AslBelgi:Login` → `AslBelgi__Login`.

| # | Kerak | Kim beradi | Qayerga | Env o'zgaruvchisi / joy | Hozirgi holat |
|---|---|---|---|---|---|
| 1 | **AuthMode** tanlash (`technical`/`business`) | Menejer / ЛК profili | config | `AslBelgi__AuthMode` | `technical` (default) |
| 2 | **Texnik** login | Buxgalter / ЛК texnik user | env (SIR) | `AslBelgi__Login` | `SET_VIA_ENVIRONMENT` |
| 3 | **Texnik** parol (90 kun) | Buxgalter / ЛК texnik user | env (SIR) | `AslBelgi__Password` | `SET_VIA_ENVIRONMENT` |
| 4 | **Biznes** apiKey (ЛК'da **E-IMZO** bilan generatsiya, 90 kun) | Buxgalter / ЛК | env (SIR) | `AslBelgi__ApiKey` | `SET_VIA_ENVIRONMENT` |
| 5 | **Prod base URL** (stage → prod) | Siz (deploy) | config | `AslBelgi__ServerBaseUrl` = `https://xtrace.aslbelgisi.uz` | stage: `xtrace.stage.aslbelgisi.uz` |
| 6 | **GTIN** qiymati (tovar bo'yicha) | Buxgalter / GTIN registri / import | DB — `inv_product.gtin` | `Product.Gtin` ustuni (bor) | ustun bor, qiymat bo'sh |
| 7 | **productGroup** (масалан `alcohol`) | Menejer / ЛК profil | request paramі | so'rov body'sida | справочник, qiymat kutadi |
| 8 | **releaseMethodType** (масалан `PRIMARY`) | Menejer / ЛК | request paramі | so'rov body'sida | справочник |
| 9 | **cisType** (масалан `UNIT`) | Menejer / ЛК | request paramі | so'rov body'sida | справочник |
| 10 | **serialNumberType** (`OPERATOR`/`SELF_MADE`) | Menejer / ЛК | request paramі | so'rov body'sida | справочник |
| 11 | **businessPlaceId** (МОД id) | Менежер / ЛК profil | request paramі | so'rov body'sida | qiymat kutadi |

> **Muhim (halol):** `apiKey` va `login` backend tomonidan **generatsiya QILINMAYDI** — ular
> ЛК'dan (E-IMZO bilan) qo'lda olinadi va env'ga qo'yiladi. Sozlanmagan bo'lsa modul aniq
> `AslBelgi.CredentialsNotConfigured` xatosini qaytaradi (jonli so'rov qilmaydi).

---

## 3. Real E2E qadamlari (credentials kelganda)

> Hozir BAJARILMAYDI — credentials/GTIN yo'q. Kelajakda quyidagi ketma-ketlik.

1. **Env**: `AslBelgi__Login` + `AslBelgi__Password` (technical) **yoki** `AslBelgi__ApiKey` (business) qo'yish.
2. **AuthMode**: `AslBelgi__AuthMode` = `technical`/`business`.
3. **Base URL**: test uchun stage (`xtrace.stage.aslbelgisi.uz`) qoldiriladi.
4. **GTIN**: bitta test tovarga `inv_product.gtin` qiymatini qo'yish (yoki request'da `Gtin` override).
5. **Order register**: `POST /api/asl-belgisi/marking/request` (productGroup, releaseMethodType, cisType, serialNumberType, businessPlaceId, productId, quantity) → **orderId** kutiladi.
6. **Codes bind**: `POST /api/asl-belgisi/marking/bind` (orderId, productId) → KM'lar `inv_product_table.marking_number`ga yozilishi, `BoundCount`/`SkippedExisting` tekshiriladi.
7. **Idempotentlik**: bind'ni qayta chaqirib, dublikat KM yozilmasligini tasdiqlash.

**Kutilgan natija:** orderId qaytadi; KM kodlar har biri alohida `ProductTable` qatoriga
(`MarkingNumber`) bog'lanadi; takroriy chaqiruv yangi qator yaratmaydi.

---

## 4. Didox + Asl Belgisi — umumiy holat

| Integratsiya | Kod holati | Test | Tashqi KUTISH NUQTALARI |
|---|---|---|---|
| **Didox (D1–D3)** | Kod tayyor (~85–90%) | Didox unit testlar yashil | `TaxIntegration__Didox__PartnerToken` (env, menejerdan); `FacturaDocType` (ЭСФ docType — dok'dan); frontend E-IMZO `companyToken` (har so'rov); prod URL `api-partners.didox.uz`; ЭСФ domain maydonlari (quyida) |
| **Asl Belgisi (A1–A5)** | Kod tayyor (~90%) | 20 test yashil | login/parol yoki apiKey (env, buxgalter/ЛК); AuthMode; GTIN; справочник paramlar; prod URL |

**Umumiy: kimdan nima kutiladi**
- **Buxgalter / ЛК:** Asl Belgisi login/parol yoki apiKey; GTIN qiymatlari; Didox ЭСФ rekvizitlari (VatRegCode, hisob/МФО, VatRegStatus va h.k.).
- **Menejer:** Didox PartnerToken (akkaunt-менежердан); Asl Belgisi справочник qiymatlari (productGroup, businessPlaceId/МОД va h.k.).
- **Siz (deploy):** prod URL'lar, env sirlarini qo'yish, ЭСФ `FacturaDocType` kodini dok'dan tasdiqlash.

Ikkalasi ham **"kod tayyor, tashqi kutadi"** holatida.

---

## 5. Qolgan placeholder / TODO (yashirilmagan)

**Asl Belgisi:**
- appsettings: `Login`/`Password`/`ApiKey` = `SET_VIA_ENVIRONMENT` (kutilyapti)
- `ServerBaseUrl` = stage (prod'ga o'zgartirish kerak)
- `AslBelgiOrderInfo` (GET /orders javobi) — to'liq sxema `TODO(Asl Belgisi doc)` bilan belgilangan
- ProductTable status: emissiya qilingan (hali qabul qilinmagan) KM uchun `IN_STOCK` ishlatilgan — kerak bo'lsa alohida "EMITTED" holat kelajakda
- A4-dan keyin ishlatilmay qolgan (harmless) qism: `AslBelgiSubmitResponse` + envelope switch shoxlari (Didox emas, AslBelgi ichki — dead branch, xatosiz)

**Didox (kontekst uchun):**
- `TaxIntegration__Didox__PartnerToken` = `SET_VIA_ENVIRONMENT`; `FacturaDocType` = bo'sh (fail-fast)
- ЭСФ `TODO(domain)`: VatRegCode, Account/BankId(МФО), VatRegStatus, Accountant, Origin, CatalogName, PackageCode, HasMarking
- ЭСФ `TODO(Didox doc)`: MeasureId→o'lchov mapping, sana formati, docType kodi

---

## 6. Xavfsizlik (tasdiq)

- ✅ Asl Belgisi sirlari (Login/Password/ApiKey) appsettings'da `SET_VIA_ENVIRONMENT` — env'dan.
- ✅ `apiKey`/`login` backend tomonidan **generatsiya qilinmaydi** — ЛК'dan (E-IMZO) qo'lda.
- ✅ Sozlanmagan credential → aniq `Result.Failure`, jonli so'rov qilinmaydi (sir sizib chiqmaydi).
- ⚠️ **Ogohlantirish (A5 doirasidan tashqari, oldindan mavjud):** `appsettings.json`da boshqa
  ba'zi sirlar ochiq matnda commit qilingan (DB `ConnectionStrings`, `FakturaAuthSettings:Password`,
  `EImzo:CertificatePassword`, `Email:Password`). Bular bu vazifada kiritilmagan — alohida
  tozalash tavsiya etiladi (env'ga ko'chirish).

---

## 7. Yakuniy xulosa

Asl Belgisi moduli **yopildi (tashqi kutadi)**: to'liq oqim — token (A1) → order/codes (A2) →
marking bog'lash (A3) — kod darajasida ulangan, DRY refaktor (A4) qilingan, 20 unit test yashil.
Modul jonli ishlashi uchun faqat **2-bo'limdagi KUTISH NUQTALARI** (credentials, GTIN, справочник)
tashqaridan berilishi kerak; keyin **3-bo'limdagi E2E qadamlari** stage'da bajariladi.
