import fs from 'fs'
import path from 'path'

const walkDir = (dir, callback) => {
  fs.readdirSync(dir).forEach(f => {
    let dirPath = path.join(dir, f)
    let isDirectory = fs.statSync(dirPath).isDirectory()
    isDirectory ? walkDir(dirPath, callback) : callback(path.join(dir, f))
  })
}

const processFile = (filePath) => {
  if (!filePath.endsWith('.vue')) return
  
  let content = fs.readFileSync(filePath, 'utf-8')
  if (!content.includes('const getStatusColor =') || content.includes('utils/status')) {
    return
  }

  console.log(`Processing: ${filePath}`)

  // Determine what type of status this page handles based on its path
  let importNameColor = ''
  let importNameLabel = ''
  
  if (filePath.includes('customers') || filePath.includes('users') || filePath.includes('accounts')) {
    importNameColor = 'getUserStatusColor'
    importNameLabel = 'getUserStatusLabel'
  } else if (filePath.includes('bills')) {
    importNameColor = 'getBillStatusColor'
    importNameLabel = 'getBillStatusLabel'
  } else {
    // transfers, transactions, payments
    importNameColor = 'getTransactionStatusColor'
    importNameLabel = 'getTransactionStatusLabel'
  }

  // Find the exact block to remove using a loop or robust regex
  // We want to remove from 'const getStatusColor = (status: string) => {' to the matching '}'
  const removeBlock = (code, startPattern) => {
    const startIndex = code.indexOf(startPattern)
    if (startIndex === -1) return code
    
    // Find the end brace '}'
    let braceCount = 0
    let i = startIndex
    let foundFirstBrace = false
    
    for (; i < code.length; i++) {
        if (code[i] === '{') {
            braceCount++
            foundFirstBrace = true
        } else if (code[i] === '}') {
            braceCount--
            if (foundFirstBrace && braceCount === 0) {
                break
            }
        }
    }
    
    if (i < code.length) {
        // also remove trailing newlines
        let endIndex = i + 1
        while (endIndex < code.length && (code[endIndex] === '\r' || code[endIndex] === '\n')) {
            endIndex++
        }
        return code.substring(0, startIndex) + code.substring(endIndex)
    }
    return code
  }

  content = removeBlock(content, 'const getStatusColor =')
  content = removeBlock(content, 'const getStatusLabel =')

  // Insert the import statement after the last standard import in <script setup>
  const importStatement = `import { ${importNameColor} as getStatusColor, ${importNameLabel} as getStatusLabel } from '~/utils/status'`
  
  if (content.includes("import { formatDateTime }")) {
      content = content.replace("import { formatDateTime } from '~/utils/date'", `import { formatDateTime } from '~/utils/date'\n${importStatement}`)
  } else if (content.includes("import { useRoute }")) {
      content = content.replace("import { useRoute } from 'vue-router'", `import { useRoute } from 'vue-router'\n${importStatement}`)
  } else if (content.includes("import { ref")) {
      content = content.replace("import { ref } from 'vue'", `import { ref } from 'vue'\n${importStatement}`)
  }

  fs.writeFileSync(filePath, content, 'utf-8')
  console.log(`Updated: ${filePath}`)
}

walkDir('./frontend/pages', processFile)
