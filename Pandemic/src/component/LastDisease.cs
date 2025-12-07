using Unity.Entities;

namespace Pandemic
{
	public struct LastDisease : IComponentData, IQueryTypeParameter
	{
        public Entity lastDisease1;
        public Entity lastDisease2;
        public Entity lastDisease3;
        public Entity lastDisease4;

        public byte lastDiseaseIndex;

		public bool IsInLastDiseases(Entity diseaseEntity)
		{
			return diseaseEntity == this.lastDisease1 ||
				   diseaseEntity == this.lastDisease2 ||
				   diseaseEntity == this.lastDisease3 ||
				   diseaseEntity == this.lastDisease4;
		}

		public void setLastDisease(Entity diseaseEntity)
		{
			switch (this.lastDiseaseIndex)
			{
				case 0:
					this.lastDisease1 = diseaseEntity;
					break;
				case 1:
					this.lastDisease2 = diseaseEntity;
					break;
				case 2:
					this.lastDisease3 = diseaseEntity;
					break;
				case 3:
					this.lastDisease4 = diseaseEntity;
					break;
			}

			this.lastDiseaseIndex = (byte)((this.lastDiseaseIndex + 1) % 4);
		}
	}
}
