# Prompt — Publish LocalLink lên GitHub và Setup Team Workflow

Bạn đang làm việc trong **repository local hiện tại của project LocalLink**.

Project đã hoàn thành Milestone 1 và hiện có đầy đủ:

- ASP.NET Core .NET 10 backend
- Nuxt 4 + Vue 3 + TypeScript + Quasar frontend
- SQL Server 2022
- Entity Framework Core
- Docker / Docker Compose
- README
- `.env.example`
- `.gitignore`

## GitHub Owner

GitHub username của tôi là:

```text
hoi936
```

GitHub account:

```text
https://github.com/hoi936
```

Repo profile `hoi936/hoi936` KHÔNG phải repository của LocalLink.

Phải tạo **repository mới riêng biệt**:

```text
hoi936/LocalLink
```

---

# 1. MỤC TIÊU

Thực hiện toàn bộ Git/GitHub setup cho LocalLink:

```text
Local project
    ↓
Git repository
    ↓
GitHub Public Repository
hoi936/LocalLink
    ↓
main
    ↓
dev
    ├── feature/hoi
    └── feature/huy
```

Sau đó mời GitHub user:

```text
HuyBong2925
```

làm collaborator của repository.

GitHub profile của collaborator:

```text
https://github.com/HuyBong2925
```

---

# 2. KIỂM TRA GITHUB CLI

Trước tiên chạy:

```bash
gh --version
gh auth status
```

Phải xác nhận GitHub CLI đang authenticated.

Tài khoản authenticated mong muốn:

```text
hoi936
```

Nếu `gh` chưa được cài hoặc chưa authenticated:

DỪNG các thao tác GitHub remote và báo chính xác vấn đề.

KHÔNG tự tạo token.

KHÔNG yêu cầu tôi gửi Personal Access Token trong chat.

Nếu authenticated bằng tài khoản khác, không tạo repository nhầm owner.

---

# 3. KIỂM TRA GIT LOCAL

Chạy:

```bash
git status
git branch --show-current
git remote -v
```

Không xóa code hiện tại.

Không reset project.

Không chạy:

```bash
git reset --hard
git clean -fd
```

---

# 4. SECURITY CHECK TRƯỚC KHI PUSH

Đây là bước bắt buộc.

Kiểm tra `.gitignore`.

Đảm bảo các file sau KHÔNG được commit:

```text
.env
.env.local
*.secret
node_modules/
.nuxt/
.output/
bin/
obj/
.vs/
.idea/
```

`.env.example` được phép commit.

Kiểm tra:

```bash
git check-ignore .env
```

Nếu `.env` đang được track:

```bash
git rm --cached .env
```

KHÔNG xóa `.env` khỏi máy local.

---

# 5. KIỂM TRA SECRET

Search toàn bộ source trước khi push để đảm bảo không có:

```text
real passwords
GitHub tokens
JWT secrets
API keys
production connection strings
private certificates
```

Đặc biệt kiểm tra:

```text
DB_PASSWORD
MSSQL_SA_PASSWORD
ConnectionStrings
Authorization:
Bearer
ghp_
github_pat_
```

Nếu phát hiện secret thật:

DỪNG trước khi push và báo file + vị trí.

Không in toàn bộ secret ra terminal/report.

Có thể redact giá trị.

---

# 6. INITIALIZE GIT NẾU CẦN

Nếu project chưa là Git repository:

```bash
git init
```

Đặt default branch:

```bash
git branch -M main
```

Nếu repository đã được initialize thì giữ history hiện tại.

Không tạo repository Git mới bên trong repository hiện tại.

---

# 7. COMMIT MILESTONE 1

Nếu Milestone 1 chưa được commit:

```bash
git add .
git status
```

Xác nhận `.env` không nằm trong staged files.

Sau đó commit:

```bash
git commit -m "feat: initialize LocalLink project foundation"
```

Nếu đã có commit tương ứng thì KHÔNG tạo commit duplicate.

---

# 8. TẠO GITHUB REPOSITORY

Repository cần tạo:

```text
hoi936/LocalLink
```

Visibility:

```text
PUBLIC
```

Description:

```text
Cloud-native regional banking and local services simulation platform built with ASP.NET Core, Nuxt, SQL Server and Docker.
```

Trước khi tạo, kiểm tra repository đã tồn tại chưa:

```bash
gh repo view hoi936/LocalLink
```

Nếu chưa tồn tại, tạo:

```bash
gh repo create hoi936/LocalLink \
  --public \
  --description "Cloud-native regional banking and local services simulation platform built with ASP.NET Core, Nuxt, SQL Server and Docker." \
  --source=. \
  --remote=origin \
  --push
```

Nếu repository đã tồn tại:

KHÔNG tạo repo duplicate.

Kiểm tra remote và kết nối project local với repo hiện tại.

---

# 9. REMOTE ORIGIN

Remote mong muốn:

```text
origin -> https://github.com/hoi936/LocalLink.git
```

Kiểm tra:

```bash
git remote -v
```

Nếu `origin` chưa tồn tại:

```bash
git remote add origin https://github.com/hoi936/LocalLink.git
```

Nếu `origin` đang trỏ repo sai:

KHÔNG đổi ngay lập tức một cách mù quáng.

Kiểm tra URL hiện tại trước.

Nếu chắc chắn đây là LocalLink project và remote cũ không đúng, sửa:

```bash
git remote set-url origin https://github.com/hoi936/LocalLink.git
```

---

# 10. PUSH MAIN

Đảm bảo branch chính là:

```text
main
```

Push:

```bash
git push -u origin main
```

Verify:

```bash
git status
git branch -vv
```

---

# 11. TẠO DEV BRANCH

Từ `main`:

```bash
git checkout main
git pull origin main
git checkout -b dev
git push -u origin dev
```

Branch:

```text
main
└── dev
```

`main` dùng cho version ổn định.

`dev` dùng để tích hợp development.

---

# 12. TẠO FEATURE BRANCH CHO TÔI

Tạo branch của owner:

```bash
git checkout dev
git checkout -b feature/hoi
git push -u origin feature/hoi
```

Branch này thuộc workflow của:

```text
hoi936
```

---

# 13. TẠO FEATURE BRANCH CHO HUY

Tạo:

```bash
git checkout dev
git checkout -b feature/huy
git push -u origin feature/huy
```

Branch dùng cho collaborator:

```text
HuyBong2925
```

Sau khi tạo xong, quay lại:

```bash
git checkout dev
```

Local working branch cuối cùng:

```text
dev
```

---

# 14. MỜI COLLABORATOR

Mời:

```text
HuyBong2925
```

vào:

```text
hoi936/LocalLink
```

Dùng GitHub API thông qua `gh`.

Thực hiện:

```bash
gh api \
  --method PUT \
  repos/hoi936/LocalLink/collaborators/HuyBong2925
```

Nếu GitHub trả về invitation created thì xem như thành công.

Không cấp admin permission.

Mục tiêu là collaborator có quyền phát triển/push phù hợp mặc định của personal repository.

Sau khi gửi invitation, kiểm tra:

```bash
gh api repos/hoi936/LocalLink/invitations
```

Xác nhận invitation dành cho:

```text
HuyBong2925
```

đang pending nếu user chưa accept.

Không cố tự accept thay người dùng HuyBong2925.

---

# 15. WORKFLOW LÀM VIỆC

Workflow từ giờ:

```text
main
  ↑
  │ Pull Request khi milestone ổn định
  │
dev
  ↑
  ├── feature/hoi
  └── feature/huy
```

### hoi936

Làm trên:

```text
feature/hoi
```

### HuyBong2925

Làm trên:

```text
feature/huy
```

Khi hoàn thành feature:

```text
feature/*
    ↓
Pull Request
    ↓
dev
```

Sau khi milestone hoàn chỉnh:

```text
dev
    ↓
Pull Request
    ↓
main
```

Không push feature trực tiếp vào `main`.

---

# 16. BRANCH PROTECTION

Nếu quyền GitHub/API hiện tại cho phép, ưu tiên thiết lập hoặc hướng dẫn thiết lập:

### main

Không làm việc trực tiếp.

Merge thông qua Pull Request.

### dev

Là integration branch.

Feature branch merge vào đây.

Không bắt buộc bật protection nếu GitHub account/repository settings không hỗ trợ phù hợp.

Không làm fail toàn task chỉ vì branch protection không cấu hình được.

---

# 17. CONVENTIONAL COMMITS

Team sử dụng:

```text
feat:
fix:
refactor:
test:
docs:
chore:
ci:
```

Ví dụ:

```text
feat: add customer entity
feat: implement account management
feat: implement transfer engine
fix: prevent transfer with insufficient balance
test: add transfer integration tests
docs: update architecture diagram
```

---

# 18. KHÔNG CODE FEATURE

Task này CHỈ làm Git/GitHub.

Không:

```text
implement database milestone
implement authentication
implement customer
implement account
implement transfer
modify frontend
upgrade packages
change architecture
```

Không thay đổi source application trừ trường hợp cần sửa `.gitignore` hoặc documentation Git workflow.

---

# 19. VERIFY REPOSITORY

Sau khi hoàn thành, verify:

```bash
gh repo view hoi936/LocalLink
```

Verify repository là:

```text
PUBLIC
```

Verify branches:

```bash
git ls-remote --heads origin
```

Phải có:

```text
main
dev
feature/hoi
feature/huy
```

Verify collaborator/invitation bằng GitHub API.

---

# 20. DEFINITION OF DONE

Chỉ báo task hoàn thành khi:

```text
[ ] GitHub CLI authenticated as hoi936

[ ] Repository hoi936/LocalLink tồn tại

[ ] Repository visibility = Public

[ ] Source LocalLink Milestone 1 đã push

[ ] .env không được commit

[ ] Không có secret thật trong repository

[ ] main tồn tại trên GitHub

[ ] dev tồn tại trên GitHub

[ ] feature/hoi tồn tại

[ ] feature/huy tồn tại

[ ] HuyBong2925 đã được gửi collaborator invitation

[ ] origin trỏ đúng hoi936/LocalLink

[ ] Local branch cuối cùng = dev

[ ] git status clean
```

---

# 21. FINAL REPORT

Sau khi hoàn thành, trả cho tôi:

```text
LOCALINK GITHUB SETUP REPORT

Repository:
https://github.com/hoi936/LocalLink

Visibility:
Public

Owner:
hoi936

Collaborator:
HuyBong2925

Branches:
- main
- dev
- feature/hoi
- feature/huy

Current Local Branch:
dev

Milestone 1 Commit:
<commit hash>

Remote:
<origin>

Collaborator Invitation:
<sent / accepted / pending>

Secret Check:
<pass/fail>

.env Tracking:
<ignored/not ignored>

Git Status:
<clean/not clean>
```

Nếu invitation đang pending, ghi rõ:

```text
HuyBong2925 must accept the GitHub invitation.
```

Sau đó DỪNG.

Không bắt đầu Milestone 2.