# BioBlocks Avatar Prompts

Use estes prompts em DALL-E 3 ou Midjourney para gerar os 10 avatares do BioBlocks.

**Configurações recomendadas:**
- Resolução: 256x256 (ou gerar em 1024x1024 e reduzir depois)
- Formato de saída: PNG com fundo transparente
- No DALL-E 3: pedir "transparent background" nem sempre funciona. Gere com fundo branco sólido e remova depois (ex: remove.bg ou Photoshop).

**Estilo base (incluir em todos os prompts):**
> Flat design cartoon character, cute kawaii style, simple shapes, soft pastel colors, white or solid color background, no outlines, minimal detail, suitable for a mobile app avatar, biology/science theme, friendly expression

---

## 1. DNA Helix
```
Flat design cartoon character of a cute smiling DNA double helix, kawaii style, soft pastel blue and pink colors, the helix has big friendly eyes and a small smile, simple shapes, minimal detail, white solid background, suitable for a mobile app avatar icon
```

## 2. Célula Animal
```
Flat design cartoon character of a cute animal cell, kawaii style, soft pastel purple and pink colors, visible nucleus with a happy face, small organelles inside, simple rounded blob shape, big friendly eyes, minimal detail, white solid background, suitable for a mobile app avatar icon
```

## 3. Bactéria (E. coli)
```
Flat design cartoon character of a cute rod-shaped bacterium with flagella, kawaii style, soft pastel green color, big friendly eyes and a cheerful smile, small flagella trailing behind, simple shapes, minimal detail, white solid background, suitable for a mobile app avatar icon
```

## 4. Vírus (Bacteriófago)
```
Flat design cartoon character of a cute bacteriophage virus, kawaii style, soft pastel orange and yellow colors, geometric head with big friendly eyes and a playful smile, thin legs, simple shapes, minimal detail, white solid background, suitable for a mobile app avatar icon
```

## 5. Proteína
```
Flat design cartoon character of a cute folded protein structure, kawaii style, colorful alpha helices and beta sheets in soft pastel rainbow colors, big friendly eyes, simple ribbon-like shape, minimal detail, white solid background, suitable for a mobile app avatar icon
```

## 6. Mitocôndria
```
Flat design cartoon character of a cute mitochondria, kawaii style, soft pastel orange and coral colors, bean-shaped with visible inner membrane folds (cristae), big energetic eyes and a confident smile, simple shapes, minimal detail, white solid background, suitable for a mobile app avatar icon
```

## 7. Ribossomo
```
Flat design cartoon character of a cute ribosome, kawaii style, soft pastel yellow and teal colors, two rounded subunits stacked together, big friendly eyes on the large subunit, small smile, simple shapes, minimal detail, white solid background, suitable for a mobile app avatar icon
```

## 8. Microscópio
```
Flat design cartoon character of a cute laboratory microscope, kawaii style, soft pastel blue and silver colors, big friendly eyes on the eyepiece lens, small smile on the body, simple shapes, minimal detail, white solid background, suitable for a mobile app avatar icon
```

## 9. Neurônio
```
Flat design cartoon character of a cute neuron cell, kawaii style, soft pastel purple and lavender colors, round cell body with big friendly eyes and a happy smile, branching dendrites and a long axon, simple shapes, minimal detail, white solid background, suitable for a mobile app avatar icon
```

## 10. Célula Vegetal
```
Flat design cartoon character of a cute plant cell, kawaii style, soft pastel green colors, rectangular shape with visible cell wall, happy face, small green chloroplasts inside, simple shapes, minimal detail, white solid background, suitable for a mobile app avatar icon
```

---

## Pós-processamento

Após gerar cada imagem:
1. Remover o fundo branco (usar remove.bg, Photoshop, ou GIMP)
2. Redimensionar para 256x256 pixels
3. Salvar como PNG com transparência
4. Nomear como: `avatar_dna.png`, `avatar_cell.png`, `avatar_bacteria.png`, `avatar_virus.png`, `avatar_protein.png`, `avatar_mitochondria.png`, `avatar_ribosome.png`, `avatar_microscope.png`, `avatar_neuron.png`, `avatar_plant_cell.png`
5. Colocar em: `Assets/Resources/AvatarPresets/`
