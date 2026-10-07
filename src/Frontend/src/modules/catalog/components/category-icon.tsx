"use client";

import { Boxes } from "lucide-react";
import { DynamicIcon, iconNames, type IconName } from "lucide-react/dynamic";

type CategoryIconProps = {
  iconKey?: string;
  className?: string;
};

const knownIconNames = new Set<string>(iconNames);

export function CategoryIcon({ iconKey, className }: CategoryIconProps) {
  const name = iconKey?.trim().toLowerCase() ?? "";

  if (!knownIconNames.has(name)) return <Boxes aria-hidden="true" className={className} />;

  return <DynamicIcon name={name as IconName} aria-hidden="true" className={className} fallback={() => <Boxes aria-hidden="true" className={className} />} />;
}
